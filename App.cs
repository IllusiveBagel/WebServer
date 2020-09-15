using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebServer.Models;
using WebServer.Services;
using WebServer.Interfaces;

namespace WebServer
{
    public class App
    {
        private readonly Config _config;
        private readonly ILogger<App> _logger;
        private readonly IServer _webServer;

        public App(IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<App>();
            _config = config.Get<Config>();
            _webServer = new Server("http://localhost:51111/", _config.webroot, loggerFactory);
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
            finally { _webServer.Stop(); }
        }
    }
}