using DocxJaTranslator.Models;

namespace DocxJaTranslator.Interfaces;

/// <summary>
/// Interface for sentence segmentation
/// </summary>
public interface ISegmenter
{
    /// <summary>
    /// Builds sentence-level segments from text nodes
    /// </summary>
    /// <param name="nodes">Collection of text nodes</param>
    /// <returns>Collection of segments</returns>
    IEnumerable<Segment> Build(IEnumerable<TextNode> nodes);
}
