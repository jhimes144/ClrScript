using ClrScript.LS;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                        .MinimumLevel.Verbose()
                        .CreateLogger();

Log.Logger.Information("ClrScript language server started.");

var server = await LanguageServer.From(options =>
    options
        .WithInput(Console.OpenStandardInput())
        .WithOutput(Console.OpenStandardOutput())
        .ConfigureLogging(
            x => x
            .AddSerilog(Log.Logger)
            .AddLanguageProtocolLogging()
            .SetMinimumLevel(LogLevel.Debug)
        )
        //.WithHandler<TextDocumentSyncHandler>()
        //.WithHandler<CompletionHandler>()
        .WithHandler<HoverHandler>()
// Add more handlers as needed
);

await server.WaitForExit;