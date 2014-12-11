using System;

namespace CfgDotNet.Test.Model
{
    public class ElasticsearchSettings
    {
        public string User { get; set; }
        public string Password { get; set; }
        public Uri BaseUrl { get; set; }
        public string DefaultIndex { get; set; }
    }
}
