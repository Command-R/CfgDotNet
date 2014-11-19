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
        private readonly string _activeEnvironmentName;
        private Dictionary<string, object> _configSections;
        private CfgEnvironment _activeEnvironment;

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
            _activeEnvironmentName = File.ReadAllText(Path.Combine(dir, "environment.txt")).Trim();
            if (!_cfgContainer.Environments.ContainsKey(_activeEnvironmentName))
            {
                throw new Exception("There is not a proper active environment configured for CfgDotNet");
            }
        }

        public CfgManager(FileInfo fileInfo, string environmentName)
            : this(File.ReadAllText(fileInfo.FullName), environmentName)
        {

        }

        public CfgManager(string json, string environmentName)
        {
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json);
            _activeEnvironmentName = environmentName;
            _activeEnvironment = _cfgContainer.Environments[_activeEnvironmentName];
            _configSections = _activeEnvironment;
        }

        public Dictionary<string, CfgConnectionSetting> ConnectionStrings
        {
            get
            {
                return ((JObject)
                    _cfgContainer.Environments[_activeEnvironmentName]["connectionStrings"]).ToObject<Dictionary<string, CfgConnectionSetting>>();
            }
        }

        public Dictionary<string, string> AppSettings
        {
            get 
            {
                return ((JObject) _cfgContainer.Environments[_activeEnvironmentName]["appSettings"]).ToObject<Dictionary<string, string>>();
            }
        }

        public object this[string key]
        {
            get
            {
                // todo: best way to handle getting from JObject to object?
                return ((JObject)_cfgContainer.Environments[_activeEnvironmentName][key]).ToObject<object>();
            }
        }

        public T GetConfigSection<T>(string key)
        {
            return ((JObject)_cfgContainer.Environments[_activeEnvironmentName][key]).ToObject<T>();
        }

        public T GetConfigSection<T>(string key, T settings)
        {
            var obj = ((JObject) _cfgContainer.Environments[_activeEnvironmentName][key]);
            JsonConvert.PopulateObject(obj.ToString(), settings);
            return settings;
        }

        public string ActiveEnvironmentName
        {
            get { return _activeEnvironmentName; }
        }
    }
}