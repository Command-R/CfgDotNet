using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public CfgManager(FileInfo fileInfo, string environment)
        {
            string json = File.ReadAllText(fileInfo.FullName);
            var obj = JObject.Parse(json);
            
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json);
            _activeEnvironment = environment;
        }

        public CfgManager(string json, string environment)
        {
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json);
            _activeEnvironment = environment;
        }

        public Dictionary<string, CfgConnectionSetting> ConnectionStrings
        {
            get { return _cfgContainer.Environments[_activeEnvironment].ConnectionStrings; }
        }

        //public static class AppSettingsEx
        //{
        //    public static T Get<T>(string keyName)
        //    {
        //        return (T) _cfgContainer.Environments[_activeEnvironment].AppSettings;
        //    }
        //}

        public Dictionary<string, string> AppSettings
        {
            get { return _cfgContainer.Environments[_activeEnvironment].AppSettings; }
        }

        public string ActiveEnvironment
        {
            get { return _activeEnvironment; }
        }
    }
}