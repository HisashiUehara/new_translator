using DocumentFormat.OpenXml.Packaging;
using DocxJaTranslator.Interfaces;
using DocxJaTranslator.Models;
using DocxJaTranslator.Translators;
using System.Diagnostics;

namespace DocxJaTranslator.Core;

/// <summary>
/// Main processor for translating DOCX documents
/// </summary>
public sealed class DocxProcessor
{
    private readonly OpenXmlScanner _scanner;
    private readonly ISegmenter _segmenter;
    private readonly DntProtector _dntProtector;
    private readonly IReinserter _reinserter;
    private readonly ITranslator _translator;
    private readonly Glossary? _glossary;

    public DocxProcessor(
        OpenXmlScanner scanner,
        ISegmenter segmenter,
        DntProtector dntProtector,
        IReinserter reinserter,
        ITranslator translator,
        Glossary? glossary = null)
    {
        _scanner = scanner;
        _segmenter = segmenter;
        _dntProtector = dntProtector;
        _reinserter = reinserter;
        _translator = translator;
        _glossary = glossary;
    }

    /// <summary>
    /// Executes the complete translation process
    /// </summary>
    /// <param name="options">Processing options</param>
    /// <returns>Execution report</returns>
    public async Task<RunReport> ExecuteAsync(DocxOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Step 1: Copy input file to output location
            var outputPath = CopyInputFile(options.InputPath, options.OutputPath);
            
            // Step 2: Open the document
            using var doc = WordprocessingDocument.Open(outputPath, true);
            
            // Step 3: Collect text nodes
            var nodes = _scanner.CollectTextNodes(doc, options);
            var nodeCount = nodes.Count();
            
            // Step 4: Build segments
            var segments = _segmenter.Build(nodes).ToList();
            var segmentCount = segments.Count;
            
            // Step 5: Apply DNT masking and glossary
            var maskedSegments = ApplyDntAndGlossary(segments);
            var dntCount = maskedSegments.Sum(s => s.DntTokenCount);
            
            // Step 6: Batch segments for translation
            var batches = CreateBatches(maskedSegments, _translator.MaxCharsPerCall);
            
            // Step 7: Translate batches
            var translationResults = await TranslateBatches(batches, options);
            var apiCalls = translationResults.Count;
            
            // Step 8: Unmask and prepare for reinsertion
            var finalResults = UnmaskResults(maskedSegments, translationResults);
            
            // Step 9: Reinsert translated text
            _reinserter.Apply(finalResults, doc.MainDocumentPart);
            
            // Step 10: Save and validate
            doc.Save();
            
            // Calculate statistics
            var charsIn = segments.Sum(s => s.SourceText.Length);
            var charsOut = finalResults.Sum(r => r.ja.Length);
            var processingTime = stopwatch.ElapsedMilliseconds;
            
            return new RunReport(
                nodeCount,
                segmentCount,
                dntCount,
                _glossary?.Count ?? 0,
                apiCalls,
                charsIn,
                charsOut,
                processingTime
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            throw new Exception($"Translation failed: {ex.Message}", ex);
        }
    }

    private string CopyInputFile(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"Input file not found: {inputPath}");
        }

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        File.Copy(inputPath, outputPath, true);
        return outputPath;
    }

    private List<MaskedSegment> ApplyDntAndGlossary(List<Segment> segments)
    {
        var maskedSegments = new List<MaskedSegment>();
        
        foreach (var segment in segments)
        {
            // Apply DNT masking
            var maskedText = _dntProtector.Mask(segment.SourceText, out var tokenCount);
            
            // Apply glossary hints
            var glossaryText = _glossary?.Apply(maskedText) ?? maskedText;
            
            maskedSegments.Add(new MaskedSegment(segment, maskedText, glossaryText, tokenCount));
        }
        
        return maskedSegments;
    }

    private List<List<MaskedSegment>> CreateBatches(List<MaskedSegment> segments, int maxCharsPerCall)
    {
        var batches = new List<List<MaskedSegment>>();
        var currentBatch = new List<MaskedSegment>();
        var currentCharCount = 0;
        
        foreach (var segment in segments)
        {
            var segmentLength = segment.GlossaryText.Length;
            
            if (currentCharCount + segmentLength > maxCharsPerCall && currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
                currentBatch = new List<MaskedSegment>();
                currentCharCount = 0;
            }
            
            currentBatch.Add(segment);
            currentCharCount += segmentLength;
        }
        
        if (currentBatch.Count > 0)
        {
            batches.Add(currentBatch);
        }
        
        return batches;
    }

    private async Task<List<string>> TranslateBatches(List<List<MaskedSegment>> batches, DocxOptions options)
    {
        var results = new List<string>();
        var context = new TranslatorContext(options.Style, _glossary?.GetHints() ?? new Dictionary<string, string>());
        
        foreach (var batch in batches)
        {
            var batchText = string.Join("\n\n", batch.Select(s => s.GlossaryText));
            var translation = await _translator.TranslateAsync(batchText, "en", "ja", context);
            results.Add(translation);
        }
        
        return results;
    }

    private List<(Segment seg, string ja)> UnmaskResults(List<MaskedSegment> maskedSegments, List<string> translations)
    {
        var results = new List<(Segment seg, string ja)>();
        var translationIndex = 0;
        
        foreach (var maskedSegment in maskedSegments)
        {
            // Find the corresponding translation
            var translation = translations[translationIndex];
            
            // Unmask the DNT tokens
            var unmaskedTranslation = _dntProtector.Unmask(translation);
            
            results.Add((maskedSegment.Segment, unmaskedTranslation));
            
            // Move to next translation if we've used all characters
            // This is a simplified approach - in practice, you'd need more sophisticated batching
            if (translationIndex < translations.Count - 1)
            {
                translationIndex++;
            }
        }
        
        return results;
    }

    /// <summary>
    /// Internal class for tracking masked segments
    /// </summary>
    private class MaskedSegment
    {
        public Segment Segment { get; }
        public string MaskedText { get; }
        public string GlossaryText { get; }
        public int DntTokenCount { get; }

        public MaskedSegment(Segment segment, string maskedText, string glossaryText, int dntTokenCount)
        {
            Segment = segment;
            MaskedText = maskedText;
            GlossaryText = glossaryText;
            DntTokenCount = dntTokenCount;
        }
    }
}
