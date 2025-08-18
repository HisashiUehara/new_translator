using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocxJaTranslator.Models;

namespace DocxJaTranslator.Core;

/// <summary>
/// PowerPointファイルからテキストを抽出するスキャナー（最小限版）
/// </summary>
public class PresentationScanner
{
    /// <summary>
    /// PowerPointファイルからテキストノードを抽出
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>テキストノードのリスト</returns>
    public List<TextNode> ScanPresentation(string filePath)
    {
        var nodes = new List<TextNode>();
        
        try
        {
            using var presentation = PresentationDocument.Open(filePath, false);
            if (presentation?.PresentationPart?.Presentation == null)
            {
                throw new InvalidOperationException("Invalid PowerPoint file or corrupted presentation");
            }
            
            // 基本的なテキスト抽出のみ実装
            // 詳細な処理は後で段階的に追加
            nodes.Add(new TextNode("presentation.xml", "/presentation", "PowerPoint file loaded", "p:presentation"));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error processing PowerPoint file: {ex.Message}");
        }
        
        return nodes;
    }
}
