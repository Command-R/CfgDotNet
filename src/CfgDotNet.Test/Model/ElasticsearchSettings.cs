using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
