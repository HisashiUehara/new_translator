using System.CommandLine;
using DocxJaTranslator.Core;
using DocxJaTranslator.Interfaces;
using DocxJaTranslator.Models;
using DocxJaTranslator.Translators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DocxJaTranslator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Docx JA Translator - Translate English Word documents to Japanese");

        // Define options
        var inputOption = new Option<string>(
            "--input",
            "Path to input DOCX file")
        { IsRequired = true };

        var outputOption = new Option<string>(
            "--output",
            "Path for output DOCX file")
        { IsRequired = true };

        var glossaryOption = new Option<string?>(
            "--glossary",
            "Path to glossary file (TSV format)");

        var engineOption = new Option<string>(
            "--engine",
            "Translation engine: deepl, openai, or local")
        { DefaultValueFactory = () => "openai" };

        var translateCommentsOption = new Option<bool>(
            "--translate-comments",
            "Translate comments")
        { DefaultValueFactory = () => true };

        var noHeadersOption = new Option<bool>(
            "--no-headers",
            "Skip header translation")
        { DefaultValueFactory = () => false };

        var noFootnotesOption = new Option<bool>(
            "--no-footnotes",
            "Skip footnote translation")
        { DefaultValueFactory = () => false };

        var keepTrackChangesOption = new Option<bool>(
            "--keep-track-changes",
            "Keep Track Changes as-is")
        { DefaultValueFactory = () => true };

        var styleOption = new Option<string>(
            "--style",
            "Translation style (e.g., tech-ja-keitei)")
        { DefaultValueFactory = () => "tech-ja-keitei" };

        var configOption = new Option<string?>(
            "--config",
            "Path to YAML configuration file");

        var reportOption = new Option<string?>(
            "--report",
            "Path for JSON report output");

        var logOption = new Option<string?>(
            "--log",
            "Path for log output");

        // Add options to command
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(glossaryOption);
        rootCommand.AddOption(engineOption);
        rootCommand.AddOption(translateCommentsOption);
        rootCommand.AddOption(noHeadersOption);
        rootCommand.AddOption(noFootnotesOption);
        rootCommand.AddOption(keepTrackChangesOption);
        rootCommand.AddOption(styleOption);
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(reportOption);
        rootCommand.AddOption(logOption);

        // Set handler
        rootCommand.SetHandler(async (input, output, glossary, engine, translateComments, noHeaders, noFootnotes, keepTrackChanges, style, config, report, log) =>
        {
            try
            {
                await ProcessDocument(input, output, glossary, engine, translateComments, noHeaders, noFootnotes, keepTrackChanges, style, config, report, log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, inputOption, outputOption, glossaryOption, engineOption, translateCommentsOption, noHeadersOption, noFootnotesOption, keepTrackChangesOption, styleOption, configOption, reportOption, logOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ProcessDocument(
        string inputPath,
        string outputPath,
        string? glossaryPath,
        string engine,
        bool translateComments,
        bool noHeaders,
        bool noFootnotes,
        bool keepTrackChanges,
        string style,
        string? configPath,
        string? reportPath,
        string? logPath)
    {
        // Setup logging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            if (!string.IsNullOrEmpty(logPath))
            {
                // Add file logging if path specified
                builder.AddFile(logPath);
            }
        });

        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Starting Docx JA Translator");
        logger.LogInformation("Input: {InputPath}", inputPath);
        logger.LogInformation("Output: {OutputPath}", outputPath);
        logger.LogInformation("Engine: {Engine}", engine);

        // Load configuration
        var configuration = LoadConfiguration(configPath);
        
        // Create options
        var options = new DocxOptions(
            inputPath,
            outputPath,
            glossaryPath,
            engine,
            translateComments,
            !noHeaders,
            !noFootnotes,
            keepTrackChanges,
            style,
            configPath
        );

        // Setup services
        var services = new ServiceCollection();
        ConfigureServices(services, configuration, engine);
        var serviceProvider = services.BuildServiceProvider();

        // Get required services
        var processor = serviceProvider.GetRequiredService<DocxProcessor>();
        var glossary = !string.IsNullOrEmpty(glossaryPath) ? Glossary.Load(glossaryPath) : null;

        // Process document
        var report = await processor.ExecuteAsync(options);

        // Write report
        if (!string.IsNullOrEmpty(reportPath))
        {
            await WriteReport(report, reportPath);
        }

        // Display summary
        Console.WriteLine($"\nTranslation completed successfully!");
        Console.WriteLine($"Nodes processed: {report.NodeCount}");
        Console.WriteLine($"Segments created: {report.SegmentCount}");
        Console.WriteLine($"DNT tokens: {report.DntCount}");
        Console.WriteLine($"Glossary hits: {report.GlossaryHits}");
        Console.WriteLine($"API calls: {report.ApiCalls}");
        Console.WriteLine($"Characters in: {report.CharsIn}");
        Console.WriteLine($"Characters out: {report.CharsOut}");
        Console.WriteLine($"Processing time: {report.ProcessingTimeMs}ms");
        Console.WriteLine($"Output saved to: {outputPath}");
    }

    static IConfiguration LoadConfiguration(string? configPath)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
        {
            builder.AddYamlFile(configPath, optional: false);
        }

        return builder.Build();
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration, string engine)
    {
        // Core services
        services.AddSingleton<OpenXmlScanner>();
        services.AddSingleton<ISegmenter, SentenceSegmenter>();
        services.AddSingleton<DntProtector>();
        services.AddSingleton<IReinserter, TextReinserter>();

        // Translator factory
        services.AddTransient<ITranslator>(provider =>
        {
            return engine.ToLower() switch
            {
                "openai" => CreateOpenAiTranslator(configuration),
                "deepl" => CreateDeepLTranslator(configuration),
                "local" => throw new NotImplementedException("Local translation not yet implemented"),
                _ => throw new ArgumentException($"Unknown translation engine: {engine}")
            };
        });

        // Main processor
        services.AddTransient<DocxProcessor>();
    }

    static ITranslator CreateOpenAiTranslator(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? 
                     Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
                     throw new InvalidOperationException("OpenAI API key not found. Set OPENAI_API_KEY environment variable or configure in appsettings.json");

        var model = configuration["OpenAI:Model"] ?? "gpt-4";
        return new OpenAiTranslator(apiKey, model);
    }

    static ITranslator CreateDeepLTranslator(IConfiguration configuration)
    {
        var apiKey = configuration["DeepL:ApiKey"] ?? 
                     Environment.GetEnvironmentVariable("DEEPL_API_KEY") ??
                     throw new InvalidOperationException("DeepL API key not found. Set DEEPL_API_KEY environment variable or configure in appsettings.json");

        var useProApi = configuration.GetValue<bool>("DeepL:UseProApi", true);
        return new DeepLTranslator(apiKey, useProApi);
    }

    static async Task WriteReport(RunReport report, string reportPath)
    {
        var reportDir = Path.GetDirectoryName(reportPath);
        if (!string.IsNullOrEmpty(reportDir) && !Directory.Exists(reportDir))
        {
            Directory.CreateDirectory(reportDir);
        }

        var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(reportPath, reportJson);
    }
}
