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
        {
            IsRequired = true
        };

        var outputOption = new Option<string>(
            "--output",
            "Path to output DOCX file")
        {
            IsRequired = true
        };

        var configOption = new Option<string?>(
            "--config",
            "Path to JSON configuration file");

        var glossaryOption = new Option<string?>(
            "--glossary",
            "Path to TSV glossary file");

        var logOption = new Option<string?>(
            "--log",
            "Path to log file");

        var verboseOption = new Option<bool>(
            "--verbose",
            "Enable verbose logging");

        var dryRunOption = new Option<bool>(
            "--dry-run",
            "Perform translation without saving");

        var forceOption = new Option<bool>(
            "--force",
            "Overwrite output file if it exists");

        var helpOption = new Option<bool>(
            "--help",
            "Show help information");

        // Add options to command
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(glossaryOption);
        rootCommand.AddOption(logOption);
        rootCommand.AddOption(verboseOption);
        rootCommand.AddOption(dryRunOption);
        rootCommand.AddOption(forceOption);
        rootCommand.AddOption(helpOption);

        // Set handler
        rootCommand.SetHandler(async (input, output, config, glossary, log, verbose, dryRun, force) =>
        {
            try
            {
                await ProcessDocument(input, output, config, glossary, log, verbose, dryRun, force);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, inputOption, outputOption, configOption, glossaryOption, logOption, verboseOption, dryRunOption, forceOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ProcessDocument(
        string inputPath,
        string outputPath,
        string? configPath,
        string? glossaryPath,
        string? logPath,
        bool verbose,
        bool dryRun,
        bool force
        )
    {
        // Setup logging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            if (!string.IsNullOrEmpty(logPath))
            {
                // Add file logging if path specified
                Console.WriteLine($"Logging to: {logPath}");
            }
        });

        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Starting Docx JA Translator");
        logger.LogInformation("Input: {InputPath}", inputPath);
        logger.LogInformation("Output: {OutputPath}", outputPath);

        // Load configuration
        var configuration = LoadConfiguration(configPath);
        
        // Create options
        var options = new DocxOptions(
            inputPath,
            outputPath,
            glossaryPath,
            "openai", // Default engine for now
            true, // Default translate comments
            true, // Default no headers
            true, // Default no footnotes
            true, // Default keep track changes
            "tech-ja-keitei", // Default style
            configPath
        );

        // Setup services
        var services = new ServiceCollection();
        ConfigureServices(services, configuration, "openai"); // Default engine for now
        var serviceProvider = services.BuildServiceProvider();

        // Get required services
        var processor = serviceProvider.GetRequiredService<DocxProcessor>();
        var glossary = !string.IsNullOrEmpty(glossaryPath) ? Glossary.Load(glossaryPath) : null;

        // Process document
        var report = await processor.ExecuteAsync(options);

        // Write report
        if (!string.IsNullOrEmpty(logPath))
        {
            await WriteReport(report, logPath);
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
        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

        if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
        {
            if (configPath.EndsWith(".json"))
            {
                builder.AddJsonFile(configPath, optional: false);
            }
            else
            {
                Console.WriteLine($"Warning: Configuration file {configPath} is not JSON. Only appsettings.json and environment variables will be used.");
            }
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
