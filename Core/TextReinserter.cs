using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using DocxJaTranslator.Interfaces;
using DocxJaTranslator.Models;
using System.Xml;

namespace DocxJaTranslator.Core;

/// <summary>
/// Reinserts translated text into OpenXML documents while preserving formatting
/// </summary>
public sealed class TextReinserter : IReinserter
{
    /// <summary>
    /// Applies translation results to the document
    /// </summary>
    /// <param name="results">Translation results with segments and Japanese text</param>
    /// <param name="root">Root OpenXML part container</param>
    public void Apply(IEnumerable<(Segment seg, string ja)> results, OpenXmlPartContainer root)
    {
        foreach (var (segment, japaneseText) in results)
        {
            ApplySegment(segment, japaneseText, root);
        }
    }

    private void ApplySegment(Segment segment, string japaneseText, OpenXmlPartContainer root)
    {
        if (segment.Nodes.Count == 0) return;

        // Apply the translated text to the first run, clear subsequent runs
        var firstNode = segment.Nodes[0];
        var remainingNodes = segment.Nodes.Skip(1).ToList();

        // Find and update the first text element
        UpdateTextElement(firstNode, japaneseText, root);

        // Clear text from remaining nodes while preserving formatting
        foreach (var node in remainingNodes)
        {
            ClearTextElement(node, root);
        }
    }

    private void UpdateTextElement(TextNode node, string text, OpenXmlPartContainer root)
    {
        try
        {
            if (node.Kind == "w:t")
            {
                UpdateWordprocessingText(node, text, root);
            }
            else if (node.Kind == "a:t")
            {
                UpdateDrawingText(node, text, root);
            }
        }
        catch (Exception ex)
        {
            // Log error but continue processing
            Console.WriteLine($"Warning: Failed to update text element {node.XPath}: {ex.Message}");
        }
    }

    private void UpdateWordprocessingText(TextNode node, string text, OpenXmlPartContainer root)
    {
        // Parse XPath to find the text element
        var xpathParts = ParseXPath(node.XPath);
        
        if (root is WordprocessingDocument wordDoc)
        {
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart?.Document?.Body != null)
            {
                var textElement = FindTextElementByXPath(mainPart.Document.Body, xpathParts);
                if (textElement != null)
                {
                    textElement.Text = text;
                }
            }
        }
    }

    private void UpdateDrawingText(TextNode node, string text, OpenXmlPartContainer root)
    {
        // Handle DrawingML text elements
        var xpathParts = ParseXPath(node.XPath);
        
        if (root is WordprocessingDocument wordDoc)
        {
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart?.Document?.Body != null)
            {
                var drawingElement = FindDrawingElementByXPath(mainPart.Document.Body, xpathParts);
                if (drawingElement != null)
                {
                    var textElement = drawingElement.Descendants<Drawing.Text>().FirstOrDefault();
                    if (textElement != null)
                    {
                        textElement.Text = text;
                    }
                }
            }
        }
    }

    private void ClearTextElement(TextNode node, OpenXmlPartContainer root)
    {
        try
        {
            if (node.Kind == "w:t")
            {
                ClearWordprocessingText(node, root);
            }
            else if (node.Kind == "a:t")
            {
                ClearDrawingText(node, root);
            }
        }
        catch (Exception ex)
        {
            // Log error but continue processing
            Console.WriteLine($"Warning: Failed to clear text element {node.XPath}: {ex.Message}");
        }
    }

    private void ClearWordprocessingText(TextNode node, OpenXmlPartContainer root)
    {
        var xpathParts = ParseXPath(node.XPath);
        
        if (root is WordprocessingDocument wordDoc)
        {
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart?.Document?.Body != null)
            {
                var textElement = FindTextElementByXPath(mainPart.Document.Body, xpathParts);
                if (textElement != null)
                {
                    textElement.Text = string.Empty;
                }
            }
        }
    }

    private void ClearDrawingText(TextNode node, OpenXmlPartContainer root)
    {
        var xpathParts = ParseXPath(node.XPath);
        
        if (root is WordprocessingDocument wordDoc)
        {
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart?.Document?.Body != null)
            {
                var drawingElement = FindDrawingElementByXPath(mainPart.Document.Body, xpathParts);
                if (drawingElement != null)
                {
                    var textElement = drawingElement.Descendants<Drawing.Text>().FirstOrDefault();
                    if (textElement != null)
                    {
                        textElement.Text = string.Empty;
                    }
                }
            }
        }
    }

    private string[] ParseXPath(string xpath)
    {
        return xpath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                   .Select(part => part.Replace("w:", "").Replace("a:", ""))
                   .ToArray();
    }

    private Text? FindTextElementByXPath(OpenXmlElement root, string[] xpathParts)
    {
        var current = root;
        
        foreach (var part in xpathParts)
        {
            if (current == null) break;
            
            if (part == "t")
            {
                return current as Text;
            }
            
            current = current.Elements().FirstOrDefault(e => e.LocalName == part);
        }
        
        return null;
    }

    private Drawing? FindDrawingElementByXPath(OpenXmlElement root, string[] xpathParts)
    {
        var current = root;
        
        foreach (var part in xpathParts)
        {
            if (current == null) break;
            
            if (part == "drawing")
            {
                return current as Drawing;
            }
            
            current = current.Elements().FirstOrDefault(e => e.LocalName == part);
        }
        
        return null;
    }
}
