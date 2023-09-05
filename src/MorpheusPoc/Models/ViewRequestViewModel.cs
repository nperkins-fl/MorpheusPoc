using System.Collections.Generic;

namespace MorpheusPoc.Models
{
    public class ViewRequestViewModel
    {
        public IDictionary<string, string> RequestHeaders { get; set; }

        public IDictionary<string, string> ResponseHeaders { get; set; }

        public IDictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> ServerVariables { get; set;}
    }
}