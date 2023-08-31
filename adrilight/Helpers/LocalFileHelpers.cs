using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace adrilight.Helpers
{
    public class LocalFileHelpers
    {
        public LocalFileHelpers()
        {

        }
        /// <summary>
        /// import file from disk
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T OpenImportFileDialog<T>(string ext, string filter)
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = ext;
            Import.Filter = filter;
            Import.FilterIndex = 1;
            Import.ShowDialog();
            T item;
            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                var json = File.ReadAllText(Import.FileName);

                try
                {
                    item = JsonConvert.DeserializeObject<T>(json);
                    return item;
                }
                catch (Exception)
                {
                    HandyControl.Controls.MessageBox.Show("Corrupted or incompatible data File!!!", "File Import", MessageBoxButton.OK, MessageBoxImage.Error);
                    return default(T);
                }
            }
            return default(T);
        }
        public void OpenExportFileDialog(object content, string ext, string filter, string name)
        {
            SaveFileDialog Export = new SaveFileDialog();
            Export.CreatePrompt = true;
            Export.OverwritePrompt = true;
            Export.Title = "Xuất dữ liệu";
            Export.CheckFileExists = false;
            Export.CheckPathExists = true;
            Export.DefaultExt = ext;
            Export.FileName = name;
            Export.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Export.RestoreDirectory = true;

            if (Export.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var contentjson = JsonConvert.SerializeObject(content);
                    File.WriteAllText(Export.FileName, contentjson);
                }
                catch (Exception)
                {
                    //log
                }
            }

        }
        public string OpenImportFileDialog(string ext, string filter)
        {
            OpenFileDialog Import = new OpenFileDialog();
            Import.Title = "Chọn file";
            Import.CheckFileExists = true;
            Import.CheckPathExists = true;
            Import.DefaultExt = ext;
            Import.Filter = filter;
            Import.FilterIndex = 1;
            Import.ShowDialog();
            if (!string.IsNullOrEmpty(Import.FileName) && File.Exists(Import.FileName))
            {
                return Import.FileName;
            }
            return null;
        }
        public string OpenImportFolderDialog()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok && Directory.Exists(dialog.FileName))
            {
                return dialog.FileName;
            }
            return null;
        }
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {

                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    if (Directory.Exists(newDestinationDir))
                        continue;
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
