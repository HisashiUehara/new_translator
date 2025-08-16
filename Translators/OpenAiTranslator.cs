using DocxJaTranslator.Interfaces;
using DocxJaTranslator.Models;
using System.Text;
using System.Text.Json;

namespace DocxJaTranslator.Translators;

/// <summary>
/// OpenAI GPT-based translator adapter
/// </summary>
public sealed class OpenAiTranslator : ITranslator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    
    public string Name => "OpenAI";
    public int MaxCharsPerCall => 1500;

    public OpenAiTranslator(string apiKey, string model = "gpt-4")
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _model = model;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// <summary>
    /// Translates text using OpenAI GPT
    /// </summary>
    public async Task<string> TranslateAsync(string text, string src, string dst, TranslatorContext ctx)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        try
        {
            var systemPrompt = BuildSystemPrompt(ctx);
            var userPrompt = BuildUserPrompt(text, ctx);

            var request = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.1, // Low temperature for consistent output
                max_tokens = 2000
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);
            
            return responseObj?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? text;
        }
        catch (Exception ex)
        {
            // Log error and return original text
            Console.WriteLine($"OpenAI translation error: {ex.Message}");
            return text;
        }
    }

    private string BuildSystemPrompt(TranslatorContext ctx)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Role: EN→JA technical translation engine.");
        sb.AppendLine($"Style: {ctx.Style} (polite Japanese corporate technical document).");
        sb.AppendLine("Punctuation: 「、。」");
        sb.AppendLine("Keep ASCII units/symbols half-width.");
        sb.AppendLine("Do not alter {DNTn} tokens.");
        sb.AppendLine("Follow the glossary strictly.");
        sb.AppendLine("Maintain technical accuracy and terminology consistency.");
        sb.AppendLine("Use appropriate Japanese honorifics and polite form.");
        
        if (ctx.Hints.Count > 0)
        {
            sb.AppendLine("\nGlossary terms:");
            foreach (var hint in ctx.Hints)
            {
                sb.AppendLine($"{hint.Key} → {hint.Value}");
            }
        }
        
        return sb.ToString();
    }

    private string BuildUserPrompt(string text, TranslatorContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Please translate the following English text to Japanese:");
        sb.AppendLine();
        sb.AppendLine(text);
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        sb.AppendLine("- Maintain technical accuracy");
        sb.AppendLine("- Use appropriate Japanese business language");
        sb.AppendLine("- Preserve all {DNTn} tokens exactly as they appear");
        sb.AppendLine("- Follow the specified style guidelines");
        
        return sb.ToString();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    // OpenAI API response models
    private class OpenAiResponse
    {
        public OpenAiChoice[]? Choices { get; set; }
    }

    private class OpenAiChoice
    {
        public OpenAiMessage? Message { get; set; }
    }

    private class OpenAiMessage
    {
        public string? Content { get; set; }
    }
}
