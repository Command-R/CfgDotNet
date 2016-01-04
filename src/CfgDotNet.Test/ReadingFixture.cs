using System;
using CfgDotNet.Test.Model;
using NUnit.Framework;

namespace CfgDotNet.Test
{
    [TestFixture]
    public class ReadingFixture
    {
        private CfgManager _cfgManagerProd;

        [SetUp]
        public void SetUp()
        {
            var json = Util.GetEmbeddedResourceText("CfgDotNet.Test.good-cfg.json");
            if (!Util.IsValidJson(json))
            {
                throw new Exception("The good-cfg.json is not valid JSON");
            }
            _cfgManagerProd = new CfgManager(null, json);
        }

        [TestCase("MainConnection", Result = "server=prod.databaseserver.com;database=MyDB_PROD;uid=User_PROD;pwd=pa55w0rd!_PROD")]
        public string CanGetConnectionString(string connectionName)
        {
            if (_cfgManagerProd.ConnectionStrings != null) return _cfgManagerProd.ConnectionStrings[connectionName].ConnectionString;
            return null;
        }

        [TestCase("showDebugPanel", Result = false)]
        public bool CanGetAppSetting(string keyName)
        {
            return bool.Parse(_cfgManagerProd.AppSettings[keyName]);
        }

        [Test]
        public void CanGetSpecialValueSimpleType()
        {
            var username = _cfgManagerProd.GetConfigSection<ElasticsearchSettings>("elasticsearch").User;
            Assert.AreEqual("elastic-user-prod", username);
        }

        [Test]
        public void CanGetSpecialValueComplexType()
        {
            var baseUrl = _cfgManagerProd.GetConfigSection<ElasticsearchSettings>("elasticsearch").BaseUrl;
            const string url = "https://prod.fakeelasticserver.com:9200";
            Assert.AreEqual(new Uri(url), baseUrl);
        }

        [Test]
        public void CanPopulateObjectWithSpecialSection()
        {
            var settings = new ElasticsearchSettings();
            _cfgManagerProd.GetConfigSection("elasticsearch", settings);
            Assert.AreEqual("elastic-user-prod", settings.User);
        }

        [TestCase("doesn't exist", Result = false)]
        [TestCase("elasticsearch", Result = true)]
        public bool TestContainsConfigSection(string key)
        {
            return _cfgManagerProd.ContainsConfigSection(key);
        }

        [Test]
        public void CanLoadMultipleFilesForEnvironment()
        {
            var file1 = Util.GetEmbeddedResourceText("CfgDotNet.Test.good-cfg.json");
            const string file2 = @"{""activeEnvironment"":""dev""}";

            var cfg = new CfgManager(null, file1, file2);

            Assert.AreEqual("dev", cfg.ActiveEnvironmentName);
        }

        [Test]
        public void CanLoadMultipleFilesForCustomSections()
        {
            var file1 = Util.GetEmbeddedResourceText("CfgDotNet.Test.good-cfg.json");
            const string file2 = @"{""environments"":{""prod"":{""elasticsearch"":{""user"":""elastic-user-test""}}}}";

            var cfg = new CfgManager(null, file1, file2);
            var settings = new ElasticsearchSettings();
            var user = cfg.GetConfigSection("elasticsearch", settings).User;

            Assert.AreEqual("elastic-user-test", user);
        }

        [Test]
        public void CanLoadByPartialFilename()
        {
            var cfg = new CfgManager(null, @"good-cfg.json");
            Assert.AreEqual("prod", cfg.ActiveEnvironmentName);
        }

        [Test]
        public void CanLoadSectionByTypeName()
        {
            var section = _cfgManagerProd.GetConfigSection<ElasticsearchSettings>();
            Assert.AreEqual("ok", section.DefaultIndex);
        }

        [Test]
        public void CanLoadBaseEnvironmentData()
        {
            Assert.AreEqual("ok", _cfgManagerProd.AppSettings["baseOnlySetting"]);
        }

        [Test]
        public void CanBaseEnvironmentDataBeOverridden()
        {
            Assert.AreEqual("System.Data.SqlClient", _cfgManagerProd.ConnectionStrings["MainConnection"].ProviderName);
        }

        [Test]
        public void CanLoad_TimeSpan()
        {
            var timeout = _cfgManagerProd.GetConfigSection<ElasticsearchSettings>().Timeout;
            Assert.AreEqual(60000d, timeout.TotalMilliseconds);
        }
    }
}
