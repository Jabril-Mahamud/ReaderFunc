using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReaderFunc.Services;

using Microsoft.Extensions.Configuration; // Add this

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Ensure secrets.json is loaded for development
        var env = context.HostingEnvironment;
        config.AddUserSecrets<Program>(); // Loads user secrets
    })
    .ConfigureServices(services =>
    {
        // Retrieve configuration
        var config = services.BuildServiceProvider().GetService<IConfiguration>();

        // Retrieve values from secrets.json
        string elevenLabsApiKey = config["ElevenLabsApiKey"];
        string supabaseUrl = config["Supabase:URL"];
        string supabaseApiKey = config["Supabase:Key"];
        string supabaseAccessKey = config["Supabase:AccessKeyID"];
        string supabaseSecretKey = config["Supabase:Secret"];
        string bucketName = "audio";

        // Register services
        services.AddSingleton<ITtsService>(sp => new TtsService(elevenLabsApiKey));
        services.AddSingleton<ISupabaseStorageService>(sp =>
            new SupabaseStorageService(supabaseUrl, supabaseApiKey, supabaseAccessKey, supabaseSecretKey, bucketName));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
