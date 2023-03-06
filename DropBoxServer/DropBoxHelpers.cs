using Dropbox.Api;
using Dropbox.Api.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using static Dropbox.Api.Files.SearchMatchType;

namespace DropBoxServer
{
    public class DropBoxHelpers
    {
        public DropBoxHelpers() { }
        public DropboxClient Client { get; set; }
        public async Task<List<String>> GetAllFilesAddressInFolder(string folderName)  // this method get all file from dropbox adrilight App folder to temp folder
        {
            var list = await Client.Files.ListFolderAsync(string.Empty);

            // show folders then files
            var folderPath = "/" + folderName;
            var listFilesAddress = new List<String>();
            var folder = await Client.Files.ListFolderAsync(folderPath);
            foreach (var file in folder.Entries)
            {
                listFilesAddress.Add(file.PathLower);
            }
            return await Task.FromResult(listFilesAddress);



        }
        public async Task<List<BitmapImage>> GetAllImagesInFolder(string folderName)  // this method get all file from dropbox adrilight App folder to temp folder
        {

            var folderPath = "/" + folderName;
            var listBitmapImages = new List<BitmapImage>();
            var folder = await Client.Files.ListFolderAsync(folderPath);
            foreach (var file in folder.Entries)
            {
                using (var response = await Client.Files.DownloadAsync(file.PathLower))
                {
                    var fileStream = await response.GetContentAsStreamAsync();
                    var bitmapImage = StreamToImageSource(fileStream);
                    listBitmapImages.Add(bitmapImage);
                }
            }
            return await Task.FromResult(listBitmapImages);
        }
        BitmapImage StreamToImageSource(Stream stream)
        {
            var memory = new MemoryStream();
            stream.CopyTo(memory);
            using (memory)
            {
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
        public async Task<T> DownloadFile<T>(string onlinePath)
        {
            using (var response = await Client.Files.DownloadAsync(onlinePath))
            {
                var returnFile = new object();
                using (var currentdbxFileStream = await response.GetContentAsStreamAsync())
                {
                    
                   T download = DeserializeFromStream<T>(currentdbxFileStream);
                    return await Task.FromResult(download);
                }
                    

            }
        }
        public static T Deserialize<T>(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
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
        public async Task SaveFileToDisk(string onlinePath, string offlinePath, string fileName)
        {
            using (var response = await Client.Files.DownloadAsync(onlinePath))
            {
                var currentdbxFileStream = await response.GetContentAsStreamAsync();
                using (currentdbxFileStream)
                {

                    using (Stream fileStream = File.OpenWrite(Path.Combine(offlinePath, fileName)))
                    {

                        currentdbxFileStream.CopyTo(fileStream);
                    }
                }

            }
        }
        public async Task UploadFile(string onlinePath, string offlinePath)
        {

            using (var file = new FileStream(offlinePath, FileMode.Open, FileAccess.Read))
            {
                var updated = await Client.Files.UploadAsync(
                    onlinePath,
                    WriteMode.Overwrite.Instance,
                    body: file);
                Console.WriteLine("Saved {0}/{1} rev {2}", "to", "onlinePath", updated.Rev);
            }

        }
        public async Task UploadContent(string onlinePath, string content)
        {

            using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var updated = await Client.Files.UploadAsync(
                    onlinePath,
                    WriteMode.Overwrite.Instance,
                    body: mem);
                Console.WriteLine("Saved {0}/{1} rev {2}", "to", "onlinePath", updated.Rev);
            }

        }
    }
}
