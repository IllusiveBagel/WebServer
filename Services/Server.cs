using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebServer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebServer.Services
{
    class Server: IServer
    {
        private readonly ILogger<Server> _logger;
        HttpListener _listener;
        string _baseFolder;     // Web Page Folder

        public Server(string uriPrefix, string baseFolder, ILoggerFactory loggerFactory)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(uriPrefix);
            _baseFolder = baseFolder;
            _logger = loggerFactory.CreateLogger<Server>();
        }

        public async void Start()
        {
            _listener.Start();
            _logger.LogInformation("Server Started");

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

        public void Stop() { _listener.Stop(); }

        async void ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                string filename = Path.GetFileName(context.Request.RawUrl);
                string path = Path.Combine(_baseFolder, filename);
                byte[] msg;

                if(!File.Exists(path))
                {
                    _logger.LogError($"Resource Not Found: {path}");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    msg = Encoding.UTF8.GetBytes("Sorry, that page does not exist");
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    msg = File.ReadAllBytes(path);
                }
                context.Response.ContentLength64 = msg.Length;
                using(Stream s = context.Response.OutputStream)
                    await s.WriteAsync(msg, 0, msg.Length);
            }
            catch(Exception ex) { _logger.LogError($"Request Error: {ex}"); }
        }
    }
}