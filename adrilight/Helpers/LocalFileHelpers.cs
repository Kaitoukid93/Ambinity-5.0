using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

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
                    item = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
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
    }
}
