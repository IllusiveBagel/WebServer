using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

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

            // Load All Config Files and Add to List
            List<Sites> sites = new List<Sites>();
            DirectoryInfo directory = new DirectoryInfo(_config.siteConfigs);

            foreach (var file in directory.GetFiles("*.json"))
            {
                using (StreamReader fi = File.OpenText(file.FullName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    Sites site = (Sites)serializer.Deserialize(fi, typeof(Sites));
                    sites.Add(site);
                }
            }

            // Create Web Server
            _webServer = new ServerService(sites, loggerFactory);
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