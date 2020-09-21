using System.Collections.Generic;

namespace WebServer.Models
{
    public class Sites
    {
        public string URL { get; set; }
        public string WebRoot { get; set; }
        public bool UseEndpoints { get; set; }
        public List<Endpoint> Endpoints { get; set; }
    }

    public class Endpoint
    {
        public string URL { get; set; }
        public string File { get; set; }
    }
}