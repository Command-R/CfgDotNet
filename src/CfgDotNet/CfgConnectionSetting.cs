using System.ComponentModel;
using System.Globalization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace CfgDotNet
{
    public class CfgConnectionSetting
    {
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
    }
}
