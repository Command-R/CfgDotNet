using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CfgDotNet
{
    internal class CfgContainer
    {
        public CfgContainer()
        {
            Environments = new Dictionary<string, CfgEnvironment>();
        }

        public string ActiveEnvironment { get; set; }
        public string BaseEnvironment { get; set; }
        public Dictionary<string, CfgEnvironment> Environments { get; set; }
    }

    internal class CfgEnvironment : Dictionary<string, JObject>
    {
    }
}
