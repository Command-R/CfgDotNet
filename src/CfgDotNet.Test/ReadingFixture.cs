using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [TestCase("elasticsearch", "user", Result = "elastic-user-prod")]
        public string CanGetSpecialValue(string sectionName, string keyName)
        {
            return "";
        }

    }
}
