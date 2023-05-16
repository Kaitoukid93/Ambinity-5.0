using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.Helpers
{
    public class JsonHelpers
    {
        public static void WriteSimpleJson(object obj, string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                //log
            }

        }
    }
}
