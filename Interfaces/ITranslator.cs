using DocxJaTranslator.Models;

namespace DocxJaTranslator.Interfaces;

/// <summary>
/// Interface for translation engines
/// </summary>
public interface ITranslator
{
    /// <summary>
    /// Translates text from source to target language
    /// </summary>
    /// <param name="text">Text to translate</param>
    /// <param name="src">Source language code</param>
    /// <param name="dst">Target language code</param>
    /// <param name="ctx">Translation context</param>
    /// <returns>Translated text</returns>
    Task<string> TranslateAsync(string text, string src, string dst, TranslatorContext ctx);
    
    /// <summary>
    /// Gets the name of the translator
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the maximum characters per translation call
    /// </summary>
    int MaxCharsPerCall { get; }
}
