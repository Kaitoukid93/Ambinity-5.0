using System;
using System.IO;
using System.Reflection;

namespace adrilight_shared.Helpers
{
    public class ResourceHelpers
    {
        public ResourceHelpers()
        {

        }
        public void CopyResource(string resourceName, string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            try
            {
                using (Stream resource = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resource == null)
                    {
                        throw new ArgumentException("No such resource", "resourceName");
                    }
                    using (Stream output = File.OpenWrite(file))
                    {
                        resource.CopyTo(output);
                    }
                }
            }
            catch
            {

            }

        }
        public string[] GetResourceFileName()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceNames();


        }
    }
}
