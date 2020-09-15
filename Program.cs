using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using WebServer.Services;
using WebServer.Interfaces;

namespace WebServer
{
    class Program
    {
        public static IConfigurationRoot configuration;

        static int Main(string[] args)
        {
            // Initialize Serilog Logger
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                 .WriteTo.File(args[0], rollingInterval: RollingInterval.Day, retainedFileCountLimit: null)
                 .MinimumLevel.Debug()
                 .Enrich.FromLogContext()
                 .CreateLogger();

            try
            {
                // Start
                MainAsync(args).Wait();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        static async Task MainAsync(string[] args)
        {
            // Create Service Collection
            Log.Information("Creating Service Collection");
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create Service Provider
            Log.Information("Building Service Provider");
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                // Start App Service
                Log.Information("Starting Service");
                await serviceProvider.GetService<App>().Run();
                Log.Information("Ending Service");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error Running Service");
                throw ex;
            }
            finally
            {
                // Clear Log on App Close
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Add Logging
            serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
            {
                builder
                    .AddSerilog(dispose: true);
            }));

            serviceCollection.AddLogging();

            // Build Configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add Access to Generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            // Add App
            serviceCollection.AddTransient<App>();

            // Add Services
            serviceCollection.AddSingleton<IServerService, ServerService>();
        }
    }
}
