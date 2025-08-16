using System.Text;

namespace DocxJaTranslator.Core;

/// <summary>
/// Manages glossary terms for consistent translation
/// </summary>
public sealed class Glossary
{
    private readonly Dictionary<string, string> _terms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _regexTerms = new();
    
    /// <summary>
    /// Gets the number of glossary terms
    /// </summary>
    public int Count => _terms.Count + _regexTerms.Count;

    /// <summary>
    /// Loads glossary from a TSV file
    /// </summary>
    /// <param name="path">Path to glossary file</param>
    /// <returns>Loaded glossary instance</returns>
    public static Glossary Load(string path)
    {
        var glossary = new Glossary();
        
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Glossary file not found: {path}");
        }

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;
                
            var parts = line.Split('\t');
            if (parts.Length >= 2)
            {
                var english = parts[0].Trim();
                var japanese = parts[1].Trim();
                
                if (!string.IsNullOrEmpty(english) && !string.IsNullOrEmpty(japanese))
                {
                    glossary._terms[english] = japanese;
                }
            }
        }
        
        return glossary;
    }

    /// <summary>
    /// Adds a term to the glossary
    /// </summary>
    /// <param name="english">English term</param>
    /// <param name="japanese">Japanese translation</param>
    public void AddTerm(string english, string japanese)
    {
        if (!string.IsNullOrEmpty(english) && !string.IsNullOrEmpty(japanese))
        {
            _terms[english] = japanese;
        }
    }

    /// <summary>
    /// Applies glossary terms to text
    /// </summary>
    /// <param name="text">Text to process</param>
    /// <returns>Text with glossary terms applied</returns>
    public string Apply(string text)
    {
        var result = text;
        var hits = 0;
        
        foreach (var term in _terms.OrderByDescending(t => t.Key.Length))
        {
            if (result.Contains(term.Key, StringComparison.OrdinalIgnoreCase))
            {
                result = result.Replace(term.Key, term.Value, StringComparison.OrdinalIgnoreCase);
                hits++;
            }
        }
        
        return result;
    }

    /// <summary>
    /// Gets glossary hints for translation context
    /// </summary>
    /// <returns>Dictionary of English terms to Japanese translations</returns>
    public IReadOnlyDictionary<string, string> GetHints()
    {
        return _terms.AsReadOnly();
    }

    /// <summary>
    /// Checks if a term exists in the glossary
    /// </summary>
    /// <param name="term">Term to check</param>
    /// <returns>True if term exists</returns>
    public bool ContainsTerm(string term)
    {
        return _terms.ContainsKey(term);
    }

    /// <summary>
    /// Gets the Japanese translation for a term
    /// </summary>
    /// <param name="term">English term</param>
    /// <returns>Japanese translation or null if not found</returns>
    public string? GetTranslation(string term)
    {
        return _terms.TryGetValue(term, out var translation) ? translation : null;
    }
}
