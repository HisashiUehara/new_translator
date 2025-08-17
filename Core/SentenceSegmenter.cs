using DocxJaTranslator.Interfaces;
using DocxJaTranslator.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace DocxJaTranslator.Core;

/// <summary>
/// Implements sentence-level segmentation for translation
/// </summary>
public sealed class SentenceSegmenter : ISegmenter
{
    private static readonly Regex SentenceBoundaryPattern = new(
        @"(?<=[.!?:])\s+(?=[A-Z])|(?<=[.!?:])\s*\n|(?<=[.!?:])\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline
    );
    
    private static readonly HashSet<string> NonSentenceEndings = new(StringComparer.OrdinalIgnoreCase)
    {
        "e.g.", "i.e.", "U.S.", "No.", "Dr.", "Mr.", "Mrs.", "Ms.", "vs.", "etc.", "Inc.", "Ltd.", "Corp."
    };

    /// <summary>
    /// Builds sentence-level segments from text nodes
    /// </summary>
    /// <param name="nodes">Collection of text nodes</param>
    /// <returns>Collection of segments</returns>
    public IEnumerable<Segment> Build(IEnumerable<TextNode> nodes)
    {
        var nodeList = nodes.ToList();
        if (nodeList.Count == 0)
            yield break;

        var currentSegment = new List<TextNode>();
        var currentText = new StringBuilder();
        var segmentIndex = 0;
        var currentPartUri = nodeList[0].PartUri;

        foreach (var node in nodeList)
        {
            // Check if we need to start a new segment (different part or heading style)
            if (ShouldStartNewSegment(node, currentPartUri, currentText.ToString()))
            {
                if (currentSegment.Count > 0)
                {
                    yield return CreateSegment(currentSegment, currentText.ToString(), currentPartUri, segmentIndex++);
                    currentSegment.Clear();
                    currentText.Clear();
                }
                currentPartUri = node.PartUri;
            }

            currentSegment.Add(node);
            currentText.Append(node.RawText);

            // Check if we've reached a sentence boundary
            if (IsSentenceBoundary(currentText.ToString()))
            {
                yield return CreateSegment(currentSegment, currentText.ToString(), currentPartUri, segmentIndex++);
                currentSegment.Clear();
                currentText.Clear();
            }
        }

        // Don't forget the last segment
        if (currentSegment.Count > 0)
        {
            yield return CreateSegment(currentSegment, currentText.ToString(), currentPartUri, segmentIndex);
        }
    }

    private bool ShouldStartNewSegment(TextNode node, string currentPartUri, string currentText)
    {
        // Different part URI
        if (node.PartUri != currentPartUri)
            return true;

        // Heading style (starts with "Heading")
        if (IsHeadingStyle(node))
            return true;

        // List item (starts with bullet or number)
        if (IsListItem(node))
            return true;

        return false;
    }

    private bool IsHeadingStyle(TextNode node)
    {
        // This would need to be enhanced to check actual paragraph styles
        // For now, we'll use a simple heuristic based on text length and content
        return node.RawText.Length < 100 && 
               !node.RawText.EndsWith('.') && 
               !node.RawText.EndsWith('?') && 
               !node.RawText.EndsWith('!');
    }

    private bool IsListItem(TextNode node)
    {
        var text = node.RawText.TrimStart();
        return text.StartsWith("•") || text.StartsWith("-") || text.StartsWith("*") ||
               Regex.IsMatch(text, @"^\d+\.\s");
    }

    private bool IsSentenceBoundary(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Check for sentence-ending punctuation
        if (text.EndsWith('.') || text.EndsWith('!') || text.EndsWith('?') || text.EndsWith(':'))
        {
            // Check if it's not a non-sentence ending
            foreach (var ending in NonSentenceEndings)
            {
                if (text.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        // Check for line breaks that might indicate sentence boundaries
        if (text.Contains('\n') || text.Contains('\r'))
        {
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 1)
            {
                var lastLine = lines.Last().Trim();
                return lastLine.EndsWith('.') || lastLine.EndsWith('!') || lastLine.EndsWith('?') || lastLine.EndsWith(':');
            }
        }

        return false;
    }

    private Segment CreateSegment(List<TextNode> nodes, string sourceText, string partUri, int index)
    {
        var segmentId = $"{partUri}#{index}";
        var maskedText = sourceText; // Will be masked later by DNT protector
        
        return new Segment(segmentId, new List<TextNode>(nodes), sourceText, maskedText);
    }
}
