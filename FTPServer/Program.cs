using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            listFiles();
        }
        private static void listFiles()
        {
            string host = @"103.148.57.184";
            string username = "adrilight_publicuser";
            string password = @"@drilightPublic";

            string remoteDirectory = "/home/adrilight_enduser/ftp/files/ColorPalettes";

            using (SftpClient sftp = new SftpClient(host, 22, username, password))
            {
                try
                {
                    sftp.Connect();

                    var files = sftp.ListDirectory(remoteDirectory);

                    foreach (var file in files)
                    {
                        Console.WriteLine(file.Name);
                        using (Stream stream = new MemoryStream())
                        {
                            sftp.DownloadFile(remoteDirectory + "/" + file.Name, stream);
                        }


                    }
                    sftp.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                }
            }
        }
    }
}
