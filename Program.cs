using System;
using System.IO;
using System.Net;
using System.Text;

namespace WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Listen on port 51111, serving files in c:\webroot
            var server = new WebServer("http://localhost:51111/", @"c:\webroot");

            try
            {
                // Run the Web Server
                server.Start();
                Console.WriteLine("Server Running... Press Enter to Stop");
                Console.ReadLine();
            }
            finally { server.Stop(); }
        }
    }
}
