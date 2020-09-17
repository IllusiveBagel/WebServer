using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using WebServer.Models;
using WebServer.Interfaces;

namespace WebServer.Services
{
    class ServerService: IServerService
    {
        private readonly ILogger<ServerService> _logger;    // Logger
        HttpListener _listener;                             // Http Listener
        List<Sites> _sites;                                 // List of Site Configurations

        public ServerService(List<Sites> sites, ILoggerFactory loggerFactory)
        {
            // Add Logger
            _logger = loggerFactory.CreateLogger<ServerService>();

            // Create Listener
            _listener = new HttpListener();

            foreach(Sites site in sites)
            {
                _listener.Prefixes.Add($"http://{site.URL}/");
            }

            // Assign Sites List
            _sites = sites;
        }

        public async void Start()
        {
            // Start Listener
            _listener.Start();
            _logger.LogInformation("Server Started");

            // Read Request From Listener
            while(true)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    Task.Run(() => ProcessRequestAsync(context));
                }
                catch(HttpListenerException)        { break; } // Listener Stopped
                catch(InvalidOperationException)    { break; } // Listener Stopped
            }
        }

        // Public Facing Stop Method
        public void Stop() { _listener.Stop(); }

        async void ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                foreach(Sites site in _sites)
                {
                    if(context.Request.UserHostName == site.URL)
                    {
                        // Get Page Name From URL and Get File
                        string filename = Path.GetFileName(context.Request.RawUrl);
                        string path = Path.Combine(site.WebRoot, filename);
                        byte[] msg;

                        if(!File.Exists(path))
                        {
                            // Return 404 Page if File Not Found
                            _logger.LogError($"Resource Not Found: {path}");
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            msg = Encoding.UTF8.GetBytes("Sorry, that page does not exist");
                        }
                        else
                        {
                            // Return Requested File
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            msg = File.ReadAllBytes(path);
                        }

                        // Return Response
                        context.Response.ContentLength64 = msg.Length;
                        using(Stream s = context.Response.OutputStream)
                        await s.WriteAsync(msg, 0, msg.Length);

                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Request Error: {ex}");
            }
        }
    }
}