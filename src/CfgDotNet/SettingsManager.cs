using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CfgDotNet
{
    /// <summary> SettingsManager loads ISettings from configured ISettingsProviders. </summary>
    public class SettingsManager
    {
        private readonly List<ISettingsProvider> _settingsProviders;
        private readonly List<ISettings> _settings;

        public SettingsManager()
        {
            _settingsProviders = new List<ISettingsProvider>();
            _settings = new List<ISettings>();
        }

        public SettingsManager AddProvider(ISettingsProvider settingsProvider)
        {
            if (settingsProvider == null)
                return this;

            _settingsProviders.Add(settingsProvider);
            return this;
        }

        public SettingsManager AddProvider<T>(Func<T, ISettingsProvider> settingsProvider) where T : ISettings
        {
            if (settingsProvider == null)
                return this;

            _settingsProviders.Add(settingsProvider(GetSetting<T>()));
            return this;
        }

        public SettingsManager InsertProvider(int index, ISettingsProvider settingsProvider)
        {
            if (settingsProvider == null)
                return this;

            _settingsProviders.Insert(index, settingsProvider);
            return this;
        }

        public SettingsManager InsertProvider<T>(int index, Func<T, ISettingsProvider> settingsProvider) where T : ISettings
        {
            if (settingsProvider == null)
                return this;

            _settingsProviders.Insert(index, settingsProvider(GetSetting<T>()));
            return this;
        }

        public SettingsManager RemoveProvider<T>() where T : ISettingsProvider
        {
            return RemoveProvider(typeof(T));
        }

        public SettingsManager RemoveProvider(Type type)
        {
            _settingsProviders.RemoveAll(x => x.GetType() == type);
            return this;
        }

        public SettingsManager AddSettings(params System.Reflection.Assembly[] assemblies)
        {
            var settings = from type in assemblies.SelectMany(x => x.GetTypes())
                           where typeof(ISettings).IsAssignableFrom(type)
                                 && !type.IsAbstract
                           select LoadSettingFromType(type);

            foreach (var setting in settings)
            {
                AddSetting(setting);
            }

            return LoadSettingsFromProviders();
        }

        public SettingsManager AddSetting<T>() where T : ISettings
        {
            var setting = LoadSettingFromType(typeof(T));
            AddSetting(setting);
            return this;
        }

        public SettingsManager AddSetting(ISettings setting)
        {
            if (setting == null)
                return this;

            var existingSetting = GetSetting(setting.GetType());
            if (existingSetting != null) //only allow setting to be added once
                return this;

            _settings.Add(setting);
            return this;
        }

        public T GetSetting<T>() where T : ISettings
        {
            return (T)GetSetting(typeof(T));
        }

        public ISettings GetSetting(Type type)
        {
            return _settings.SingleOrDefault(x => x.GetType() == type);
        }

        public SettingsManager RemoveSetting<T>() where T : ISettings
        {
            var setting = GetSetting(typeof(T));
            if (setting == null)
                return this;

            _settings.Remove(setting);
            return this;
        }

        private static ISettings LoadSettingFromType(Type type)
        {
            return (ISettings)Activator.CreateInstance(type);
        }

        public SettingsManager LoadSettingsFromProviders()
        {
            foreach (var setting in _settings)
            {
                LoadSettingFromProviders(setting);
            }
            return this;
        }

        private void LoadSettingFromProviders(ISettings settings)
        {
            foreach (var provider in _settingsProviders)
            {
                provider.Load(settings);
            }
        }

        public SettingsManager Configure<T>(Action<T> configure) where T : class, ISettings
        {
            if (configure == null)
                return this;

            var setting = GetSetting(typeof(T));
            if (setting == null)
            {
                setting = LoadSettingFromType(typeof(T));
                AddSetting(setting);
            }

            configure((T)setting);
            return this;
        }

        public SettingsManager Validate()
        {
            foreach (var setting in _settings)
            {
                if (!setting.IsDisabled)
                    setting.Validate();
            }
            return this;
        }

        public SettingsManager ForEach(Action<ISettings> action)
        {
            foreach (var setting in _settings)
            {
                if (action != null) action(setting);
            }
            return this;
        }

        public List<ISettings> GetSettings()
        {
            return _settings;
        }

        public static string GetSettingTypeName(Type type)
        {
            var declaringTypeName = type.DeclaringType != null ? type.DeclaringType.Name + "+" : string.Empty;
            return declaringTypeName + type.Name;
        }
    };

    /// <summary> ISettings provides an interface can be used to find Settings classes and Validate them. </summary>
    public interface ISettings
    {
        bool IsDisabled { get; }
        void Validate();
    };

    public abstract class BaseSettings : ISettings
    {
        public bool IsDisabled { get; set; }

        public virtual void Validate()
        {
            // don't require children to implement
        }
    };

    /// <summary> ISettingsProvider can load settings from an external source. </summary>
    public interface ISettingsProvider
    {
        void Load(ISettings setting);
    };

    public abstract class SettingsProviderBase : ISettingsProvider
    {
        protected Dictionary<string, string> Values = new Dictionary<string, string>();

        public virtual void Load(ISettings setting)
        {
            var prefix = GetSettingsName(setting) + ".";
            foreach (var prop in setting.GetType().GetProperties())
            {
                if (Values.ContainsKey(prefix + prop.Name))
                {
                    SetValue(setting, prop, Values[prefix + prop.Name]);
                }
            }
        }

        protected static string GetSettingsName(ISettings setting)
        {
            return setting == null ? null : SettingsManager.GetSettingTypeName(setting.GetType());
        }

        protected static void SetValue(Object obj, System.Reflection.PropertyInfo prop, Object value)
        {
            prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
        }
    };

    /// <summary> ConnectionStringsSettingsProvider loads settings from the Configuration file's ConnectionStrings section. </summary>
    public class ConnectionStringsSettingsProvider : SettingsProviderBase
    {
        public ConnectionStringsSettingsProvider()
        {
            foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
            {
                Values.Add(cs.Name, cs.ConnectionString);
            }
        }
    };

    /// <summary> AppSettingsSettingsProvider loads settings from the Configuration file's AppSettings section. </summary>
    public class AppSettingsSettingsProvider : SettingsProviderBase
    {
        public AppSettingsSettingsProvider()
        {
            foreach (string key in ConfigurationManager.AppSettings)
            {
                Values.Add(key, ConfigurationManager.AppSettings[key]);
            }
        }
    };

    /// <summary> SqlDatabaseSettingsProvider loads settings from a Sql database table. </summary>
    public class SqlDatabaseSettingsProvider : SettingsProviderBase
    {
        public SqlDatabaseSettingsProvider(string connectionString, string tableName = "Setting", string keyColumn = "Key", string valueColumn = "Value")
        {
            if (!IsValidConnectionString(connectionString))
                return;

            try
            {
                Values = GetValuesFromTable(connectionString, tableName, keyColumn, valueColumn);
            }
            catch
            {
                //ignore exceptions since database might not have table yet
            }
        }

        private static bool IsValidConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("Server=;Database=;Uid=;Pwd=;"))
                return false;

            return true;
        }

        private static Dictionary<string, string> GetValuesFromTable(string connectionString, string tableName, string keyColumn, string valueColumn)
        {
            var values = new Dictionary<string, string>();
            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT [{1}], [{2}] FROM [{0}]", tableName, keyColumn, valueColumn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            values.Add(reader[keyColumn].ToString(), reader[valueColumn].ToString());
                        }
                    }
                }
            }
            return values;
        }
    };

    public class CfgDotNetSettingsProvider : SettingsProviderBase
    {
        private readonly CfgManager _cfgManager;

        public CfgDotNetSettingsProvider(string environment, string fileName)
        {
            _cfgManager = new CfgManager(environment, fileName);
        }

        public override void Load(ISettings setting)
        {
            if (_cfgManager == null)
                return;

            var sectionName = GetSettingsName(setting);
            if (!_cfgManager.ContainsConfigSection(sectionName))
                return;

            _cfgManager.GetConfigSection(sectionName, setting);
        }
    };
}
