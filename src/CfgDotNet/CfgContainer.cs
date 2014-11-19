using System.Collections.Generic;

namespace CfgDotNet
{
    public class CfgContainer
    {
        public string ActiveEnvironment { get; set; }
        public Dictionary<string, CfgEnvironment> Environments { get; set; }
    }
}