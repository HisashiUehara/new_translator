using DocxJaTranslator.Models;

namespace DocxJaTranslator.Core;

/// <summary>
/// PowerPointファイル処理用インターフェース
/// </summary>
public interface IPowerPointProcessor
{
    /// <summary>
    /// PowerPointファイルを処理
    /// </summary>
    /// <param name="inputPath">入力ファイルパス</param>
    /// <param name="outputPath">出力ファイルパス</param>
    /// <param name="options">処理オプション</param>
    /// <returns>処理結果</returns>
    Task<RunReport> ProcessAsync(string inputPath, string outputPath, DocxOptions options);
    
    /// <summary>
    /// ファイルがPowerPoint形式かチェック
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <returns>PowerPoint形式の場合true</returns>
    bool IsPowerPointFile(string filePath);
}
