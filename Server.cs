using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class WebServer
    {
        HttpListener _listener;
        string _baseFolder;     // Web Page Folder

        public WebServer(string uriPrefix, string baseFolder)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(uriPrefix);
            _baseFolder = baseFolder;
        }

        public async void Start()
        {
            _listener.Start();

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
                    Console.WriteLine($"Resource Not Found: {path}");
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
            catch(Exception ex) { Console.WriteLine($"Request Error: {ex}"); }
        }
    }
}