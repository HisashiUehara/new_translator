using DocxJaTranslator.Interfaces;
using DocxJaTranslator.Models;
using System.Text;
using System.Text.Json;

namespace DocxJaTranslator.Translators;

/// <summary>
/// DeepL machine translation adapter
/// </summary>
public sealed class DeepLTranslator : ITranslator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly bool _useProApi;
    
    public string Name => "DeepL";
    public int MaxCharsPerCall => 2000;

    public DeepLTranslator(string apiKey, bool useProApi = true)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _useProApi = useProApi;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"DeepL-Auth-Key {_apiKey}");
    }

    /// <summary>
    /// Translates text using DeepL API
    /// </summary>
    public async Task<string> TranslateAsync(string text, string src, string dst, TranslatorContext ctx)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        try
        {
            var baseUrl = _useProApi ? "https://api-free.deepl.com" : "https://api.deepl.com";
            var endpoint = $"{baseUrl}/v2/translate";

            var formData = new List<KeyValuePair<string, string>>
            {
                new("text", text),
                new("source_lang", NormalizeLanguageCode(src)),
                new("target_lang", NormalizeLanguageCode(dst)),
                new("formality", GetFormalityLevel(ctx.Style))
            };

            // Add glossary if available
            if (ctx.Hints.Count > 0)
            {
                var glossaryEntries = ctx.Hints.Select(h => $"{h.Key}\t{h.Value}");
                var glossaryText = string.Join("\n", glossaryEntries);
                formData.Add(new("glossary", glossaryText));
            }

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"DeepL API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<DeepLResponse>(responseContent);
            
            return responseObj?.Translations?.FirstOrDefault()?.Text?.Trim() ?? text;
        }
        catch (Exception ex)
        {
            // Log error and return original text
            Console.WriteLine($"DeepL translation error: {ex.Message}");
            return text;
        }
    }

    private string NormalizeLanguageCode(string lang)
    {
        return lang.ToLower() switch
        {
            "en" or "english" => "EN",
            "ja" or "japanese" => "JA",
            _ => lang.ToUpper()
        };
    }

    private string GetFormalityLevel(string style)
    {
        return style.ToLower() switch
        {
            "tech-ja-keitei" or "polite" => "more",
            "casual" => "less",
            _ => "default"
        };
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    // DeepL API response models
    private class DeepLResponse
    {
        public DeepLTranslation[]? Translations { get; set; }
    }

    private class DeepLTranslation
    {
        public string? Text { get; set; }
        public string? DetectedSourceLanguage { get; set; }
    }
}
