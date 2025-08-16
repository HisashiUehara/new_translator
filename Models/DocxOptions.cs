namespace DocxJaTranslator.Models;

/// <summary>
/// Configuration options for DOCX processing
/// </summary>
/// <param name="InputPath">Path to input DOCX file</param>
/// <param name="OutputPath">Path for output DOCX file</param>
/// <param name="GlossaryPath">Optional path to glossary file</param>
/// <param name="Translator">Translation engine: "deepl" | "openai" | "local"</param>
/// <param name="TranslateComments">Whether to translate comments</param>
/// <param name="TranslateHeadersFooters">Whether to translate headers and footers</param>
/// <param name="TranslateFootnotes">Whether to translate footnotes</param>
/// <param name="KeepTrackChanges">Keep Track Changes as-is</param>
/// <param name="Style">Translation style (e.g., "tech-ja-keitei")</param>
/// <param name="ConfigPath">Optional path to YAML configuration file</param>
public record DocxOptions(
    string InputPath,
    string OutputPath,
    string? GlossaryPath = null,
    string Translator = "openai",
    bool TranslateComments = true,
    bool TranslateHeadersFooters = true,
    bool TranslateFootnotes = true,
    bool KeepTrackChanges = true,
    string Style = "tech-ja-keitei",
    string? ConfigPath = null
);
