using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgDotNet.Test.Model;
using NUnit.Framework;

namespace CfgDotNet.Test
{
    [TestFixture]
    public class ReadingFixture
    {
        private string _json;
        private CfgManager _cfgManagerProd;

        [SetUp]
        public void SetUp()
        {
            _json = Util.GetEmbeddedResourceText("CfgDotNet.Test.good-cfg.json");
            if (!Util.IsValidJson(_json))
            {
                throw new Exception("The good-cfg.json is not valid JSON");
            }
            _cfgManagerProd = new CfgManager(_json, "prod");
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
            string username = _cfgManagerProd.GetConfigSection<ElasticsearchSettings>("elasticsearch").User;
            Assert.AreEqual("elastic-user-prod", username);
        }

        [Test]
        public void CanGetSpecialValueComplexType()
        {
            Uri baseUrl = _cfgManagerProd.GetConfigSection<ElasticsearchSettings>("elasticsearch").BaseUrl;
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
    }
}
