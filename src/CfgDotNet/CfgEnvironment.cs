using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CfgDotNet
{
    internal class CfgEnvironment : Dictionary<string, JObject>
    {
        //public Dictionary<string, CfgConnectionSetting> ConnectionStrings { get; set; }
        //public Dictionary<string, string> AppSettings { get; set; }
        //public Dictionary<string, object> ConfigSections { get; set; }

        //public T GetSection<T>(string key)
        //{
        //    return (T) ConfigSections[key];
        //}

        //public Dictionary<string, object> this[string key]
        //{
        //    get { return ConfigSections[key]; }
        //}

    }
}