using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CfgDotNet
{
    public class CfgManager
    {
        public readonly string CfgFileName;
        private readonly CfgContainer _cfgContainer;

        public CfgManager()
        {
            CfgFileName = "cfg.json";

            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (dir == null)
            {
                throw new Exception("The directory path of the EntryAssembly could not be determined. (dir == null)");
            }

            //load json
            var cfgPath = Path.Combine(dir, CfgFileName);
            var json = File.ReadAllText(cfgPath);
            _cfgContainer = JsonConvert.DeserializeObject<CfgContainer>(json);

            //load environment.txt
            var environmentFile = new FileInfo(Path.Combine(dir, "environment.txt"));
            if (environmentFile.Exists)
            {
                _cfgContainer.ActiveEnvironment = File.ReadAllText(environmentFile.FullName).Trim();
            }

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
        }

        public Dictionary<string, CfgConnectionSetting> ConnectionStrings
        {
            get
            {
                return ActiveEnvironment["connectionStrings"].ToObject<Dictionary<string, CfgConnectionSetting>>();
            }
        }

        public Dictionary<string, string> AppSettings
        {
            get
            {
                return ActiveEnvironment["appSettings"].ToObject<Dictionary<string, string>>();
            }
        }

        public object this[string key]
        {
            get
            {
                // todo: best way to handle getting from JObject to object?
                return ActiveEnvironment[key].ToObject<object>();
            }
        }

        public bool ContainsConfigSection(string key)
        {
            return ActiveEnvironment.ContainsKey(key);
        }

        public T GetConfigSection<T>(string key)
        {
            return ActiveEnvironment[key].ToObject<T>();
        }

        public T GetConfigSection<T>(string key, T section)
        {
            var jObject = ActiveEnvironment[key];
            JsonConvert.PopulateObject(jObject.ToString(), section);
            return section;
        }

        public string ActiveEnvironmentName
        {
            get { return _cfgContainer.ActiveEnvironment; }
        }

        private CfgEnvironment ActiveEnvironment
        {
            get { return _cfgContainer.Environments[_cfgContainer.ActiveEnvironment]; }
        }
    }
}