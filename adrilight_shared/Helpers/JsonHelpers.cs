using Newtonsoft.Json;
using System;
using System.IO;

namespace adrilight_shared.Helpers
{
    public class JsonHelpers
    {
        public static void WriteSimpleJson(object obj, string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
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
