using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CfgDotNet.Test
{
    [TestFixture]
    public class SettingsManagerTests
    {
        private Dictionary<Type, ISettings> _container;

        [SetUp]
        public void SetUp()
        {
            const string environment = "prod";
            const string cfgPath = "good-cfg.json";
            var assemblies = new[] { typeof(SettingsManager).Assembly, GetType().Assembly };
            _container = new Dictionary<Type, ISettings>();

            new SettingsManager()
                .AddProvider(new ConnectionStringsSettingsProvider())
                .AddProvider(new AppSettingsSettingsProvider())
                .AddProvider(new CfgDotNetSettingsProvider(environment, cfgPath))
                .AddSettings(assemblies)
                .Validate()
                .ForEach(x => _container.Add(x.GetType(), x));
        }

        [Test]
        public void CanReadAppSettings()
        {
            var setting = GetInstanceFromContainer<Settings>();
            Assert.AreEqual("password", setting.SecurityKey);
        }

        [Test]
        public void CanReadConnectionStrings()
        {
            var setting = GetInstanceFromContainer<Settings>();
            Assert.AreEqual("Server=;Database=;Uid=;Pwd=;", setting.ConnectionString);
        }

        [Test]
        public void CanReadCfgDotNet()
        {
            var setting = GetInstanceFromContainer<elasticsearch>();
            Assert.AreEqual("prod_documents", setting.DefaultIndex);
        }

        [Test]
        public void CanReadEnum()
        {
            var setting = GetInstanceFromContainer<Settings>();
            Assert.AreEqual(TestEnum.Value2, setting.Enum);
        }

        [Test]
        public void CanRead_TimeSpan()
        {
            var settings = GetInstanceFromContainer<Settings>();
            Assert.AreEqual(60000d, settings.Timeout.TotalMilliseconds);
        }

        private T GetInstanceFromContainer<T>() where T : ISettings
        {
            return (T)_container[typeof (T)];
        }

        public class Settings : BaseSettings
        {
            public string SecurityKey { get; set; }
            public string ConnectionString { get; set; }
            public TestEnum Enum { get; set; }
            public TimeSpan Timeout { get; set; }
        };
    }

    public class elasticsearch : BaseSettings
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string BaseUrl { get; set; }
        public string DefaultIndex { get; set; }
    };

    public enum TestEnum
    {
        Value1,
        Value2,
    };
}
