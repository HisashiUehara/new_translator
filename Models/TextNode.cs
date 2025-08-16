namespace DocxJaTranslator.Models;

/// <summary>
/// Text node (minimal extraction unit)
/// </summary>
/// <param name="PartUri">e.g., /word/document.xml</param>
/// <param name="XPath">node location</param>
/// <param name="RawText">original text (w:t / a:t)</param>
/// <param name="Kind">"w:t" | "a:t"</param>
public record TextNode(
    string PartUri,
    string XPath,
    string RawText,
    string Kind
);
