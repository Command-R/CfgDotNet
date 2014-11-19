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
            _cfgContainer.ActiveEnvironment = File.ReadAllText(Path.Combine(dir, "environment.txt")).Trim();
            if (!_cfgContainer.Environments.ContainsKey(_cfgContainer.ActiveEnvironment))
            {
                throw new Exception("There is not a proper active environment configured for CfgDotNet");
            }
        }

        public CfgManager(FileInfo fileInfo, string environmentName = null)
            : this(File.ReadAllText(fileInfo.FullName), environmentName)
        {

        }

        public CfgManager(string json, string environmentName = null)
        {
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json);
            if (environmentName != null)
            {
                _cfgContainer.ActiveEnvironment = environmentName;
            }
            _activeEnvironment = _cfgContainer.Environments[_cfgContainer.ActiveEnvironment];
            _configSections = _activeEnvironment;
        }

        public Dictionary<string, CfgConnectionSetting> ConnectionStrings
        {
            get
            {
                return ((JObject)
                    _cfgContainer.Environments[_cfgContainer.ActiveEnvironment]["connectionStrings"]).ToObject<Dictionary<string, CfgConnectionSetting>>();
            }
        }

        public Dictionary<string, string> AppSettings
        {
            get 
            {
                return ((JObject)_cfgContainer.Environments[_cfgContainer.ActiveEnvironment]["appSettings"]).ToObject<Dictionary<string, string>>();
            }
        }

        public object this[string key]
        {
            get
            {
                // todo: best way to handle getting from JObject to object?
                return ((JObject)_cfgContainer.Environments[_cfgContainer.ActiveEnvironment][key]).ToObject<object>();
            }
        }

        public bool ContainsConfigSection(string key)
        {
            return _cfgContainer.Environments[_cfgContainer.ActiveEnvironment].ContainsKey(key);
        }

        public T GetConfigSection<T>(string key)
        {
            return ((JObject)_cfgContainer.Environments[_cfgContainer.ActiveEnvironment][key]).ToObject<T>();
        }

        public T GetConfigSection<T>(string key, T section)
        {
            var jObject = ((JObject)_cfgContainer.Environments[_cfgContainer.ActiveEnvironment][key]);
            JsonConvert.PopulateObject(jObject.ToString(), section);
            return section;
        }

        public string ActiveEnvironmentName
        {
            get { return _cfgContainer.ActiveEnvironment; }
        }
    }
}