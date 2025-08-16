namespace DocxJaTranslator.Models;

/// <summary>
/// Context information for translation requests
/// </summary>
/// <param name="Style">Translation style (e.g., "tech-ja-keitei")</param>
/// <param name="Hints">Additional hints for the translator</param>
public record TranslatorContext(
    string Style,
    IReadOnlyDictionary<string, string> Hints
);
