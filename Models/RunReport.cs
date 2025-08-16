namespace DocxJaTranslator.Models;

/// <summary>
/// Run summary with execution statistics
/// </summary>
/// <param name="NodeCount">Total number of text nodes processed</param>
/// <param name="SegmentCount">Total number of segments created</param>
/// <param name="DntCount">Number of DNT tokens processed</param>
/// <param name="GlossaryHits">Number of glossary term matches</param>
/// <param name="ApiCalls">Number of translation API calls</param>
/// <param name="CharsIn">Total input characters</param>
/// <param name="CharsOut">Total output characters</param>
/// <param name="ProcessingTimeMs">Total processing time in milliseconds</param>
public record RunReport(
    int NodeCount,
    int SegmentCount,
    int DntCount,
    int GlossaryHits,
    int ApiCalls,
    int CharsIn,
    int CharsOut,
    long ProcessingTimeMs
);
