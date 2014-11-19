using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CfgDotNet.Test
{
    public static class Util
    {
        public static string GetEmbeddedResourceText(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static bool IsValidJson(string possibleJson)
        {
            try
            {
                // ReSharper disable once AccessToStaticMemberViaDerivedType
                JToken token = JContainer.Parse(possibleJson);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
