using System.Text.RegularExpressions;

namespace DocxJaTranslator.Core;

/// <summary>
/// Protects content that should not be translated (DNT - Do Not Translate)
/// </summary>
public sealed class DntProtector
{
    private readonly List<string> _tokens = new();
    private readonly Dictionary<string, string> _tokenMap = new();
    
    // DNT regex patterns as specified in the technical specification
    private static readonly Regex[] DntPatterns = {
        // URL: https?://\S+
        new(@"https?://\S+", RegexOptions.Compiled),
        
        // Email: [A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}
        new(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        
        // Version: \bv?\d+(?:\.\d+)+\b
        new(@"\bv?\d+(?:\.\d+)+\b", RegexOptions.Compiled),
        
        // Number+Unit: \b\d+(?:\.\d+)?\s?(kV|V|A|mA|W|kW|MW|Hz|%|°C|°F|mm|cm|m)\b
        new(@"\b\d+(?:\.\d+)?\s?(kV|V|A|mA|W|kW|MW|Hz|%|°C|°F|mm|cm|m)\b", RegexOptions.Compiled),
        
        // Code-like: `[^`]+`
        new(@"`[^`]+`", RegexOptions.Compiled),
        
        // Function name: \b[A-Za-z_][A-Za-z0-9_]*\(\)\b
        new(@"\b[A-Za-z_][A-Za-z0-9_]*\(\)", RegexOptions.Compiled),
        
        // HTML/XML tags: <[^>]+>
        new(@"<[^>]+>", RegexOptions.Compiled)
    };

    /// <summary>
    /// Masks DNT content with tokens
    /// </summary>
    /// <param name="text">Text to mask</param>
    /// <param name="tokenCount">Number of tokens created</param>
    /// <returns>Masked text</returns>
    public string Mask(string text, out int tokenCount)
    {
        _tokens.Clear();
        _tokenMap.Clear();
        
        var maskedText = text;
        var tokenIndex = 0;
        
        foreach (var pattern in DntPatterns)
        {
            maskedText = pattern.Replace(maskedText, match =>
            {
                var token = $"{{DNT{tokenIndex}}}";
                _tokens.Add(match.Value);
                _tokenMap[token] = match.Value;
                tokenIndex++;
                return token;
            });
        }
        
        tokenCount = _tokens.Count;
        return maskedText;
    }

    /// <summary>
    /// Unmasks DNT content by restoring original values
    /// </summary>
    /// <param name="maskedText">Text with DNT tokens</param>
    /// <returns>Unmasked text</returns>
    public string Unmask(string maskedText)
    {
        var unmaskedText = maskedText;
        
        // Restore tokens in reverse order to avoid conflicts
        for (int i = _tokens.Count - 1; i >= 0; i--)
        {
            var token = $"{{DNT{i}}}";
            if (_tokenMap.ContainsKey(token))
            {
                unmaskedText = unmaskedText.Replace(token, _tokenMap[token]);
            }
        }
        
        return unmaskedText;
    }

    /// <summary>
    /// Gets the list of DNT tokens
    /// </summary>
    public IReadOnlyList<string> Tokens => _tokens.AsReadOnly();
    
    /// <summary>
    /// Gets the token mapping
    /// </summary>
    public IReadOnlyDictionary<string, string> TokenMap => _tokenMap;
}
