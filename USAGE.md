# Docx JA Translator - Usage Guide

## Quick Examples

### Basic Translation
```bash
# Translate using OpenAI (requires OPENAI_API_KEY environment variable)
docxja --input document.docx --output document-ja.docx --engine openai

# Translate using DeepL (requires DEEPL_API_KEY environment variable)
docxja --input document.docx --output document-ja.docx --engine deepl
```

### With Glossary
```bash
# Use custom glossary for consistent terminology
docxja --input document.docx --output document-ja.docx \
       --engine openai --glossary technical-terms.tsv
```

### Custom Configuration
```bash
# Use YAML configuration file
docxja --input document.docx --output document-ja.docx \
       --config my-config.yaml --report translation-report.json
```

### Selective Translation
```bash
# Skip headers and footnotes
docxja --input document.docx --output document-ja.docx \
       --engine openai --no-headers --no-footnotes

# Keep track changes
docxja --input document.docx --output document-ja.docx \
       --engine openai --keep-track-changes
```

## Environment Setup

### OpenAI
```bash
export OPENAI_API_KEY="sk-your-openai-api-key-here"
```

### DeepL
```bash
export DEEPL_API_KEY="your-deepl-api-key-here"
```

## Configuration Files

### appsettings.json
```json
{
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000,
    "Temperature": 0.1
  },
  "DeepL": {
    "UseProApi": true,
    "Formality": "more"
  },
  "Translation": {
    "DefaultStyle": "tech-ja-keitei",
    "MaxCharsPerCall": 1500,
    "Concurrency": 3
  }
}
```

### config.yaml
```yaml
style: tech-ja-keitei
segmentation:
  headings_as_blocks: true
  list_items_as_blocks: true
translator:
  type: openai
  concurrency: 3
  max_chars_per_call: 1500
  retry:
    max_attempts: 3
    backoff_ms: 500
security:
  redact_logs: true
parts:
  charts: false
  smartart: false
  comments: true
  headers_footers: true
  footnotes: true
report:
  diff: true
  summary: true
  format: json
```

## Glossary Format

Create a TSV (Tab-Separated Values) file:

```tsv
# English	Japanese
API	API
database	データベース
server	サーバー
client	クライアント
authentication	認証
authorization	認可
encryption	暗号化
decryption	復号化
firewall	ファイアウォール
load balancer	ロードバランサー
microservice	マイクロサービス
container	コンテナ
orchestration	オーケストレーション
deployment	デプロイメント
monitoring	監視
logging	ログ記録
backup	バックアップ
restore	復元
scalability	スケーラビリティ
performance	パフォーマンス
latency	レイテンシ
throughput	スループット
availability	可用性
reliability	信頼性
maintenance	保守
upgrade	アップグレード
patch	パッチ
version	バージョン
release	リリース
development	開発
testing	テスト
production	本番環境
staging	ステージング環境
```

## Translation Styles

### tech-ja-keitei (Recommended for Technical Documents)
- Polite Japanese (敬語)
- Technical terminology consistency
- Formal business language
- Appropriate for manuals, specifications, and technical reports

### business
- Standard business Japanese
- Balanced formality
- Suitable for general business documents

### casual
- Informal Japanese
- Less formal tone
- Use with caution in professional contexts

## Output Reports

The tool generates detailed JSON reports:

```json
{
  "NodeCount": 150,
  "SegmentCount": 45,
  "DntCount": 12,
  "GlossaryHits": 8,
  "ApiCalls": 15,
  "CharsIn": 2500,
  "CharsOut": 3200,
  "ProcessingTimeMs": 45000
}
```

## Error Handling

### Common Issues

1. **API Key Missing**
   ```
   Error: OpenAI API key not found. Set OPENAI_API_KEY environment variable or configure in appsettings.json
   ```
   Solution: Set the required environment variable

2. **File Not Found**
   ```
   Error: Input file not found: document.docx
   ```
   Solution: Check file path and permissions

3. **Invalid DOCX Format**
   ```
   Error: Invalid OpenXML document format
   ```
   Solution: Ensure the file is a valid Word document (.docx)

4. **Translation API Error**
   ```
   Error: OpenAI API error: 429 - Rate limit exceeded
   ```
   Solution: Wait and retry, or reduce concurrency

### Debug Mode

Enable detailed logging:
```bash
docxja --input document.docx --output document-ja.docx \
       --engine openai --log debug.log
```

## Performance Tips

### For Large Documents
- Use appropriate `max_chars_per_call` setting
- Consider running during off-peak hours
- Monitor API rate limits

### For Batch Processing
- Use CI/CD pipelines
- Implement retry logic
- Monitor API usage and costs

## Security Considerations

### API Key Management
- Never commit API keys to source control
- Use environment variables or secure key stores
- Rotate keys regularly

### Document Privacy
- Enable `redact_logs: true` for sensitive documents
- Use local translation when possible
- Review translation service privacy policies

### Network Security
- Ensure HTTPS connections
- Use corporate proxies if required
- Monitor network traffic

## Integration Examples

### CI/CD Pipeline
```yaml
- name: Translate Documentation
  run: |
    docxja --input docs/manual.docx \
           --output docs/manual-ja.docx \
           --engine openai \
           --glossary glossary.tsv \
           --report translation-report.json
```

### Script Automation
```bash
#!/bin/bash
for file in *.docx; do
    if [[ $file != *"-ja.docx" ]]; then
        output="${file%.*}-ja.docx"
        echo "Translating $file to $output"
        docxja --input "$file" --output "$output" --engine openai
    fi
done
```

### Docker Usage
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
COPY DocxJaTranslator /app/
WORKDIR /app
ENTRYPOINT ["./DocxJaTranslator"]
```

## Troubleshooting

### Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Check .NET version
dotnet --version
```

### Runtime Issues
```bash
# Check dependencies
dotnet --info

# Run with verbose logging
dotnet run --verbosity detailed
```

### Performance Issues
- Monitor memory usage
- Check API response times
- Adjust batch sizes
- Use appropriate concurrency settings

## Support and Resources

- **Documentation**: [README.md](README.md)
- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions
- **Wiki**: Project Wiki

## Version Compatibility

- **.NET**: 8.0 or later
- **Word**: Office 2016+ compatible documents
- **OS**: Windows, macOS, Linux
- **Architecture**: x64
