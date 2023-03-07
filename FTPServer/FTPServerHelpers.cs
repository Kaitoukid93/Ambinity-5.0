using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace FTPServer
{
    public class FTPServerHelpers
    {
        public SftpClient sFTP { get; set; }
        public FTPServerHelpers()
        {

        }
        public async Task<List<String>> GetAllFilesAddressInFolder(string folderPath)
        {
            var listFilesAddress = new List<String>();

            try
            {


                var files = sFTP.ListDirectory(folderPath);

                foreach (var file in files.Where(i => i.Name != "." && i.Name != ".."))
                {
                    listFilesAddress.Add(folderPath + "/" + file.Name);

                }
                
                return await Task.FromResult(listFilesAddress);

            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has been caught " + e.ToString());
                return null;
            }
        }
    





    public async Task<T> GetFiles<T>(string filePath)
    {
            try
            {
                T file;



                using (var remoteFileStream = sFTP.OpenRead(filePath))
                {
                    var textReader = new System.IO.StreamReader(remoteFileStream);
                    string s = textReader.ReadToEnd();
                    file = JsonConvert.DeserializeObject<T>(s, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                }

                return await Task.FromResult(file);
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception has been caught " + e.ToString());
            return default(T);
        }

    }
    public static T DeserializeFromStream<T>(Stream stream)
    {
        var serializer = new JsonSerializer();
        using (var sr = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            return serializer.Deserialize<T>(jsonTextReader);
        }
    }

}
}
