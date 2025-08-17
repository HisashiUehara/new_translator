# Docx JA Translator

A professional-grade tool for translating English Word documents (.docx) into Japanese while preserving layout and functionality. Supports both cloud-based and local translation engines.

## Features

- **Layout Preservation**: Maintains all formatting, styles, tables, and links
- **Smart Segmentation**: Intelligent sentence-level text segmentation
- **DNT Protection**: Automatically protects URLs, emails, versions, numbers, and code
- **Glossary Support**: Consistent terminology through custom glossaries
- **Multiple Engines**: OpenAI GPT, DeepL, and local translation support
- **Quality Validation**: OpenXML validation and comprehensive reporting
- **CLI First**: Designed for automation and CI/CD pipelines

## Quick Start

### Prerequisites

- .NET 8.0 Runtime
- API keys for translation services (OpenAI or DeepL)

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/docx-ja-translator.git
cd docx-ja-translator

# Build the application
dotnet build

# Publish as single file
dotnet publish -c Release -r osx-x64 --self-contained true
```

### Basic Usage

```bash
# Translate a document using OpenAI
docxja --input document.docx --output document-ja.docx --engine openai

# Use DeepL with glossary
docxja --input document.docx --output document-ja.docx \
       --engine deepl --glossary glossary.tsv

# Custom configuration
docxja --input document.docx --output document-ja.docx \
       --config config.json --report report.json
```

## Configuration

### Environment Variables

```bash
# OpenAI
export OPENAI_API_KEY="your-openai-api-key"

# DeepL
export DEEPL_API_KEY="your-deepl-api-key"
```

### Configuration Files

#### appsettings.json
```json
{
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000
  },
  "DeepL": {
    "UseProApi": true
  }
}
```

#### config.json
```json
{
  "style": "tech-ja-keitei",
  "translator": {
    "type": "openai",
    "concurrency": 3,
    "max_chars_per_call": 1500
  },
  "security": {
    "redact_logs": true
  }
}
```

### Glossary Format

Create a TSV file with English terms and Japanese translations:

```tsv
API	API
database	データベース
server	サーバー
```

## Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `--input` | Input DOCX file path | Required |
| `--output` | Output DOCX file path | Required |
| `--glossary` | Glossary file path (TSV) | None |
| `--engine` | Translation engine (openai/deepl/local) | openai |
| `--translate-comments` | Translate comments | true |
| `--no-headers` | Skip header translation | false |
| `--no-footnotes` | Skip footnote translation | false |
| `--keep-track-changes` | Preserve track changes | true |
| `--style` | Translation style | tech-ja-keitei |
| `--config` | Configuration file path | None |
| `--report` | Report output path | None |
| `--log` | Log output path | None |

## Translation Styles

- **tech-ja-keitei**: Polite Japanese technical document style
- **business**: Standard business Japanese
- **casual**: Informal Japanese

## Supported Document Parts

- Main document body
- Headers and footers
- Footnotes and endnotes
- Comments
- Text boxes (DrawingML)
- Hyperlinks (display text only)

## DNT (Do Not Translate) Protection

Automatically protects:
- URLs and email addresses
- Version numbers
- Numbers with units (voltage, current, etc.)
- Code snippets and function names
- HTML/XML tags

## Performance

- **Target**: 100k characters within 5 minutes
- **Batching**: Intelligent text batching for API efficiency
- **Concurrency**: Configurable parallel processing
- **Memory**: Optimized for large documents

## Security Features

- **API Key Management**: Environment variables and secure configuration
- **Log Redaction**: Optional sensitive text masking
- **Local Processing**: Support for on-premise translation
- **HTTPS**: Encrypted API communication

## Error Handling

- **Graceful Degradation**: Continues processing on non-critical errors
- **Detailed Logging**: Comprehensive error reporting
- **Validation**: OpenXML integrity checks
- **Fallback**: Returns original text on translation failures

## Development

### Building from Source

```bash
git clone https://github.com/yourusername/docx-ja-translator.git
cd docx-ja-translator
dotnet restore
dotnet build
dotnet test
```

### Project Structure

```
DocxJaTranslator/
├── Core/                 # Core processing logic
├── Interfaces/           # Abstractions and contracts
├── Models/               # Data models
├── Translators/          # Translation engine adapters
├── Program.cs            # CLI entry point
└── DocxJaTranslator.csproj
```

### Key Components

- **OpenXmlScanner**: Extracts text nodes from DOCX
- **SentenceSegmenter**: Intelligent text segmentation
- **DntProtector**: Protects non-translatable content
- **TextReinserter**: Preserves formatting during reinsertion
- **DocxProcessor**: Orchestrates the entire process

## Testing

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/docx-ja-translator/issues)
- **Documentation**: [Wiki](https://github.com/yourusername/docx-ja-translator/wiki)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/docx-ja-translator/discussions)

## Roadmap

### v1.0 (Current)
- ✅ CLI interface
- ✅ Core translation engine
- ✅ DNT protection
- ✅ Glossary support
- ✅ Multiple translation engines

### v1.1 (Planned)
- Charts and SmartArt support
- Track Changes control
- Translation Memory (TMX)
- Term statistics dashboard

### v2.0 (Future)
- Desktop UI
- Batch folder processing
- On-premise translation server
- Advanced quality checks

## Acknowledgments

- Built with [DocumentFormat.OpenXml](https://github.com/OfficeDev/Open-XML-SDK)
- CLI framework by [System.CommandLine](https://github.com/dotnet/command-line-api)
- Translation engines: OpenAI GPT, DeepL
