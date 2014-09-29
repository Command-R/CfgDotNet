using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace CfgDotNet
{
    public class CfgManager
    {
        public readonly string CfgFileName;
        private readonly CfgContainer _cfgContainer;
        private readonly string _activeEnvironment;

        public CfgManager()
        {
            CfgFileName = "cfg.json";
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (dir == null)
            {
                throw new Exception("The directory path of the EntryAssembly could not be determined. (dir == null)");
            }
            string cfgPath = Path.Combine(dir, CfgFileName);
            string json = File.ReadAllText(cfgPath);
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json);
            _activeEnvironment = File.ReadAllText(Path.Combine(dir, "environment.txt")).Trim();
            if (!_cfgContainer.Environments.ContainsKey(_activeEnvironment))
            {
                throw new Exception("There is not a proper active environment configured for CfgDotNet");
            }
        }

        public CfgManager(string filename, string environment)
        {
            string json = File.ReadAllText(filename);
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json);
            _activeEnvironment = environment;
        }

        public CfgManager(StringBuilder json, string environment)
        {
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json.ToString());
            _activeEnvironment = environment;
        }
        
        public Dictionary<string, CfgConnectionSetting> ConnectionStrings
        {
            get { return _cfgContainer.Environments[_activeEnvironment].ConnectionStrings; }
        }

        public Dictionary<string, string> AppSettings
        {
            get { return _cfgContainer.Environments[_activeEnvironment].AppSettings; }
        }

        public string ActiveEnvironment
        {
            get { return _activeEnvironment; }
        }
    }
  
    public class CfgContainer
    {
        public Dictionary<string, Cfg> Environments { get; set; }
    }

    public class Cfg
    {
        public Dictionary<string, CfgConnectionSetting> ConnectionStrings { get; set; }
        public Dictionary<string, string> AppSettings { get; set; }
    }

    public class CfgConnectionSetting
    {
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
    }
}
