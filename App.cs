using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using WebServer.Models;
using WebServer.Services;
using WebServer.Interfaces;

namespace WebServer
{
    public class App
    {
        private readonly Config _config;            // Config From Json File
        private readonly ILogger<App> _logger;      // Logger
        private readonly IServerService _webServer; // Web Server Service

        public App(IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
            // Add Logger
            _logger = loggerFactory.CreateLogger<App>();

            // Convert Returned Config to Model
            _config = config.Get<Config>();

            // Create Web Server
            _webServer = new ServerService("http://localhost:51111/", _config.webroot, loggerFactory);
        }

        public async Task Run()
        {
            _logger.LogInformation("Task Started");

            try
            {
                // Run the Web Server
                _webServer.Start();
                _logger.LogInformation("Server Running... Press Enter to Stop");
                Console.ReadLine();
            }
            finally { _webServer.Stop(); }  // Stop Web Server on Close
        }
    }
}