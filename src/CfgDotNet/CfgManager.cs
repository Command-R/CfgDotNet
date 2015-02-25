using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CfgDotNet
{
    public class CfgManager
    {
        private readonly CfgContainer _cfgContainer;

        public CfgManager(string activeEnvironment = null, params string[] jsonPathsOrData)
        {
            var jsonStrings = LoadJsonPathsOrData(jsonPathsOrData);
            
            //load Container
            _cfgContainer = new CfgContainer();
            foreach (var json in jsonStrings)
            {
                LoadConfigurationJson(json);
            }

            //load Environment
            if (!string.IsNullOrWhiteSpace(activeEnvironment))
            {
                _cfgContainer.ActiveEnvironment = activeEnvironment;
            }
            ValidateEnvironment();
        }

        private static string FindRootDirectory()
        {
            var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
            if (entryAssembly != null)
                return Path.GetDirectoryName(entryAssembly.Location);

            var webDir = System.Web.Hosting.HostingEnvironment.MapPath("~/");
            if (webDir != null)
                return webDir;

            return Directory.GetCurrentDirectory();
        }

        private static IEnumerable<string> LoadJsonPathsOrData(ICollection<string> jsonPathsOrData)
        {
            var dir = new Lazy<string>(FindRootDirectory);
            if (jsonPathsOrData.Count == 0)
            {
                var cfgPath = Path.Combine(dir.Value, "cfg.json");
                var json = File.ReadAllText(cfgPath);
                return new[] {json};
            }

            var jsonStrings = new List<string>();
            foreach (var item in jsonPathsOrData)
            {
                if (item.Contains("{")) //isJson
                {
                    jsonStrings.Add(item);
                }
                else if (item.StartsWith("\\") || item.Contains(":")) //isFullPath
                {
                    jsonStrings.Add(File.ReadAllText(item));
                }
                else //isFilename
                {
                    jsonStrings.Add(File.ReadAllText(Path.Combine(dir.Value, item)));
                }
            }
            return jsonStrings;
        }

        private void LoadConfigurationJson(string json)
        {
            var container = JsonConvert.DeserializeObject<CfgContainer>(json);
            if (!string.IsNullOrWhiteSpace(container.ActiveEnvironment))
            {
                _cfgContainer.ActiveEnvironment = container.ActiveEnvironment;
            }

            foreach (var newEnvironmentItem in container.Environments)
            {
                //If Environment doesn't already exist, add it as-is
                if (!_cfgContainer.Environments.ContainsKey(newEnvironmentItem.Key))
                {
                    _cfgContainer.Environments[newEnvironmentItem.Key] = newEnvironmentItem.Value;
                    continue;
                }

                //We need to merge the new Environment items into the old
                var existingEnvironment = _cfgContainer.Environments[newEnvironmentItem.Key];
                var newEnvironment = newEnvironmentItem.Value;
                foreach (var newItem in newEnvironment)
                {
                    if (!existingEnvironment.ContainsKey(newItem.Key))
                    {
                        existingEnvironment[newItem.Key] = newItem.Value;
                        continue;
                    }

                    //Merge jObjects
                    var existingItem = existingEnvironment[newItem.Key];
                    existingItem.Merge(newItem.Value, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                }
            }
        }

        private void ValidateEnvironment()
        {
            var dir = FindRootDirectory() ?? string.Empty;

            //Try to load environment.txt
            var environmentFile = new FileInfo(Path.Combine(dir, "environment.txt"));
            if (environmentFile.Exists)
            {
                _cfgContainer.ActiveEnvironment = File.ReadAllText(environmentFile.FullName).Trim();
            }

            if (!_cfgContainer.Environments.ContainsKey(_cfgContainer.ActiveEnvironment))
            {
                throw new Exception("There is not a proper active environment configured for CfgDotNet: "
                                    + _cfgContainer.ActiveEnvironment);
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
                //TODO: best way to handle getting from JObject to object?
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
