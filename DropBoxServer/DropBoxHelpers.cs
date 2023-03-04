using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dropbox.Api.Files.SearchMatchType;

namespace DropBoxServer
{
    public class DropBoxHelpers
    {
        public DropBoxHelpers() { }
        public DropboxClient Client { get; set; }
        public async Task GetAllFilesInAppFolder()  // this method get all file from dropbox adrilight App folder to temp folder
        {
            var list = await Client.Files.ListFolderAsync(string.Empty);

            // show folders then files
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                Console.WriteLine(item.PathLower);
                var sharedFiles = await Client.Files.ListFolderAsync(item.PathLower);
                var destPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                foreach (var file in sharedFiles.Entries)
                {
                    await DownloadFile(file.PathLower, destPath, file.Name);
                }
            }



        }
        public async Task DownloadFile(string onlinePath, string offlinePath,string fileName)
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
        public async Task UploadFile(string onlinePath,string offlinePath)
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
