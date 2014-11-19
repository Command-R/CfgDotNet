using System.Collections.Generic;

namespace CfgDotNet
{
    internal class CfgContainer
    {
        public string ActiveEnvironment { get; set; }
        public Dictionary<string, CfgEnvironment> Environments { get; set; }
    }
}