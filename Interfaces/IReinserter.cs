using DocumentFormat.OpenXml.Packaging;
using DocxJaTranslator.Models;

namespace DocxJaTranslator.Interfaces;

/// <summary>
/// Interface for reinserting translated text into OpenXML documents
/// </summary>
public interface IReinserter
{
    /// <summary>
    /// Applies translation results to the document
    /// </summary>
    /// <param name="results">Translation results with segments and Japanese text</param>
    /// <param name="root">Root OpenXML part container</param>
    void Apply(IEnumerable<(Segment seg, string ja)> results, OpenXmlPartContainer root);
}
