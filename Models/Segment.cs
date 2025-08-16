namespace DocxJaTranslator.Models;

/// <summary>
/// Sentence-level segment
/// </summary>
/// <param name="SegmentId">{PartUri}#{seq}</param>
/// <param name="Nodes">contiguous nodes forming this sentence</param>
/// <param name="SourceText">concatenated English</param>
/// <param name="MaskedText">after DNT masking (translation input)</param>
public record Segment(
    string SegmentId,
    List<TextNode> Nodes,
    string SourceText,
    string MaskedText
);
