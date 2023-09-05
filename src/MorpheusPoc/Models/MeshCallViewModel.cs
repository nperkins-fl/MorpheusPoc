using System;
using System.Net;

namespace MorpheusPoc.Models
{
    public class MeshCallViewModel
    {
        public string Target { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
        public Exception Error { get; set; }
    }
}