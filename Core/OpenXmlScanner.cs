using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using DocxJaTranslator.Models;
using System.Xml;

namespace DocxJaTranslator.Core;

/// <summary>
/// Scans OpenXML documents to collect text nodes for translation
/// </summary>
public sealed class OpenXmlScanner
{
    /// <summary>
    /// Collects all text nodes from a Word document
    /// </summary>
    /// <param name="doc">Word document</param>
    /// <param name="options">Processing options</param>
    /// <returns>Collection of text nodes</returns>
    public IEnumerable<TextNode> CollectTextNodes(WordprocessingDocument doc, DocxOptions options)
    {
        var nodes = new List<TextNode>();
        
        // Main document body
        if (doc.MainDocumentPart?.Document?.Body != null)
        {
            nodes.AddRange(CollectFromBody(doc.MainDocumentPart.Document.Body, "/word/document.xml"));
        }

        // Headers and footers
        if (options.TranslateHeadersFooters)
        {
            if (doc.MainDocumentPart?.HeaderParts != null)
            {
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                {
                    var uri = headerPart.Uri.ToString();
                    if (headerPart.Header?.Elements() != null)
                    {
                        nodes.AddRange(CollectFromBody(headerPart.Header, uri));
                    }
                }
            }

            if (doc.MainDocumentPart?.FooterParts != null)
            {
                foreach (var footerPart in doc.MainDocumentPart.FooterParts)
                {
                    var uri = footerPart.Uri.ToString();
                    if (footerPart.Footer?.Elements() != null)
                    {
                        nodes.AddRange(CollectFromBody(footerPart.Footer, uri));
                    }
                }
            }
        }

        // Footnotes
        if (options.TranslateFootnotes && doc.MainDocumentPart?.FootnotesPart?.Footnotes != null)
        {
            nodes.AddRange(CollectFromBody(doc.MainDocumentPart.FootnotesPart.Footnotes, "/word/footnotes.xml"));
        }

        // Endnotes
        if (options.TranslateFootnotes && doc.MainDocumentPart?.EndnotesPart?.Endnotes != null)
        {
            nodes.AddRange(CollectFromBody(doc.MainDocumentPart.EndnotesPart.Endnotes, "/word/endnotes.xml"));
        }

        // Comments
        if (options.TranslateComments && doc.MainDocumentPart?.WordprocessingCommentsPart?.Comments != null)
        {
            nodes.AddRange(CollectFromBody(doc.MainDocumentPart.WordprocessingCommentsPart.Comments, "/word/comments.xml"));
        }

        return nodes;
    }

    private IEnumerable<TextNode> CollectFromBody(OpenXmlElement body, string partUri)
    {
        var nodes = new List<TextNode>();
        var xpathBuilder = new XPathBuilder();
        
        CollectTextNodesRecursive(body, partUri, xpathBuilder, nodes);
        
        return nodes;
    }

    private void CollectTextNodesRecursive(OpenXmlElement element, string partUri, XPathBuilder xpathBuilder, List<TextNode> nodes)
    {
        if (element == null) return;

        // Handle WordprocessingML text elements (w:t)
        if (element is Text textElement)
        {
            var text = textElement.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var xpath = xpathBuilder.BuildXPath();
                nodes.Add(new TextNode(partUri, xpath, text, "w:t"));
            }
        }

        // Handle DrawingML text elements (a:t)
        if (element is Drawing drawingElement)
        {
            CollectDrawingMLText(drawingElement, partUri, xpathBuilder, nodes);
        }

        // Handle hyperlinks (translate display text only)
        if (element is Hyperlink hyperlinkElement)
        {
            CollectHyperlinkText(hyperlinkElement, partUri, xpathBuilder, nodes);
        }

        // Recursively process child elements
        foreach (var child in element.Elements())
        {
            xpathBuilder.PushElement(child.LocalName);
            CollectTextNodesRecursive(child, partUri, xpathBuilder, nodes);
            xpathBuilder.PopElement();
        }
    }

    private void CollectDrawingMLText(Drawing drawing, string partUri, XPathBuilder xpathBuilder, List<TextNode> nodes)
    {
        // Navigate through DrawingML structure to find text elements
        var textElements = drawing.Descendants<Drawing.Text>();
        
        foreach (var textElement in textElements)
        {
            var text = textElement.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var xpath = xpathBuilder.BuildXPath() + "//a:t";
                nodes.Add(new TextNode(partUri, xpath, text, "a:t"));
            }
        }
    }

    private void CollectHyperlinkText(Hyperlink hyperlink, string partUri, XPathBuilder xpathBuilder, List<TextNode> nodes)
    {
        // Only collect text from hyperlink display elements, not the target
        var textElements = hyperlink.Elements<Text>();
        
        foreach (var textElement in textElements)
        {
            var text = textElement.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var xpath = xpathBuilder.BuildXPath() + "/w:hyperlink/w:t";
                nodes.Add(new TextNode(partUri, xpath, text, "w:t"));
            }
        }
    }

    /// <summary>
    /// Helper class for building XPath expressions
    /// </summary>
    private class XPathBuilder
    {
        private readonly Stack<string> _elements = new();

        public void PushElement(string elementName)
        {
            _elements.Push(elementName);
        }

        public void PopElement()
        {
            if (_elements.Count > 0)
                _elements.Pop();
        }

        public string BuildXPath()
        {
            var elements = _elements.Reverse().ToArray();
            return "/" + string.Join("/", elements.Select(e => $"w:{e}"));
        }
    }
}
