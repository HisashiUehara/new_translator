using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocxJaTranslator.Interfaces;
using DocxJaTranslator.Models;

namespace DocxJaTranslator.Core;

/// <summary>
/// PowerPointファイルに翻訳されたテキストを再挿入するクラス（最小限版）
/// </summary>
public class PresentationReinserter : IReinserter
{
    /// <summary>
    /// 翻訳結果をPowerPointファイルに適用
    /// </summary>
    /// <param name="results">翻訳結果</param>
    /// <param name="root">PowerPointドキュメント</param>
    public void Apply(IEnumerable<(Segment seg, string ja)> results, OpenXmlPartContainer root)
    {
        if (root is not PresentationDocument presentationDoc)
        {
            throw new ArgumentException("Root must be a PresentationDocument");
        }
        
        // 基本的な処理のみ実装
        // 詳細な処理は後で段階的に追加
        Console.WriteLine("PowerPoint file processing - basic implementation");
    }
    
    /// <summary>
    /// 特定のテキストノードをクリア
    /// </summary>
    /// <param name="node">テキストノード</param>
    /// <param name="root">PowerPointドキュメント</param>
    public void Clear(TextNode node, OpenXmlPartContainer root)
    {
        if (root is not PresentationDocument presentationDoc)
        {
            throw new ArgumentException("Root must be a PresentationDocument");
        }
        
        // 基本的な処理のみ実装
        // 詳細な処理は後で段階的に追加
        Console.WriteLine("PowerPoint file clearing - basic implementation");
    }
}
