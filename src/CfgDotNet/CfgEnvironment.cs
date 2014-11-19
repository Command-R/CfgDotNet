using System.Collections.Generic;

namespace CfgDotNet
{
    public class CfgEnvironment
    {
        public Dictionary<string, CfgConnectionSetting> ConnectionStrings { get; set; }
        public Dictionary<string, string> AppSettings { get; set; }
        //public Dictionary<string, object> ConfigSections { get; set; }
    }
}