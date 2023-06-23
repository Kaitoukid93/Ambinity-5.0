using Newtonsoft.Json;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


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
        public async Task<String> GetFileByName(string fileName, string folderPath)
        {
            var listFilesAddress = new List<String>();

            try
            {
                var files = sFTP.ListDirectory(folderPath);

                var file = files.Where(i => i.Name == fileName).FirstOrDefault();

                return await Task.FromResult(folderPath + "/" + file.Name);

            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has been caught " + e.ToString());
                return null;
            }
        }
        public async Task<List<SftpFile>> GetAllFilesInFolder(string folderPath)
        {
            var listFiles = new List<SftpFile>();

            try
            {


                var files = sFTP.ListDirectory(folderPath);

                foreach (var file in files.Where(i => i.Name != "." && i.Name != ".."))
                {
                    listFiles.Add(file);

                }

                return await Task.FromResult(listFiles);

            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has been caught " + e.ToString());
                return null;
            }
        }

        public async Task<BitmapImage> GetThumb(string thumbPath)  // this method get all file from dropbox adrilight App folder to temp folder
        {
            try
            {
                var thumb = new BitmapImage();

                using (var remoteFileStream = sFTP.OpenRead(thumbPath))
                {
                    thumb = StreamToImageSource(remoteFileStream);
                }

                return await Task.FromResult(thumb);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has been caught " + e.ToString());
                return null;
            }
        }
        public SftpFileAttributes GetFileAttributes(string remotePath)
        {
            SftpFileAttributes attrs = sFTP.GetAttributes(remotePath);
            return attrs;
        }
        public SftpFile GetFileOrFoldername(string remotePath)
        {
            return sFTP.Get(remotePath);
        }
        public void DownloadFile(string remotePath, string localPath, Action<ulong> donwloadCallback)  // this method get all file from dropbox adrilight App folder to temp folder
        {

            try
            {
                using (var s = File.Create(localPath))
                {
                    sFTP.DownloadFile(remotePath, s, donwloadCallback);
                }

            }
            catch (System.IO.IOException)
            {


            }
            catch (Exception ex)
            {

            }

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
                bitmapimage.Freeze();

                return bitmapimage;
            }
        }

        public async Task<T> GetFiles<T>(string filePath) //only use for text format
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
        public async Task<string> GetStringContent(string filePath) //only use for text format
        {
            try
            {
                string content;


                using (var remoteFileStream = sFTP.OpenRead(filePath))
                {
                    var textReader = new System.IO.StreamReader(remoteFileStream);
                    content = textReader.ReadToEnd();

                }

                return await Task.FromResult(content);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has been caught " + e.ToString());
                return "";
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