//using adrilight.Helpers;
//using adrilight.Settings;
//using adrilight.Spots;
using adrilight_content_creator.Model;
using adrilight_content_creator.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.Drawable;
using adrilight_shared.Models.FrameData;
using adrilight_shared.Models.Store;
using ExcelDataReader;
using FTPServer;
//using adrilight_effect_analyzer.Model;
using Newtonsoft.Json;
using Renci.SshNet;
using SharpCompress.Common;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
//using static adrilight.ViewModel.MainViewViewModel;
using File = System.IO.File;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace adrilight_content_creator.ViewModel
{
    public class ExcelItemCreatorHomeViewModel : BaseViewModel
    {
        private string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\");
        private string ExcelFileFolderPath => Path.Combine(JsonPath, "Test");

        public ExcelItemCreatorHomeViewModel()
        {
            Header = "Home";
            SetupCommand();

            ExistedExcelItemList = new ObservableCollection<IExcelItemModel>();
            foreach (var item in LoadExistedExcelItemList())
            {
                ExistedExcelItemList.Add(item);
            }


        }
        public ICommand SetSelectedOutputSlaveDeviceFromFileCommand { get; set; }


        private ObservableCollection<IExcelItemModel> _existedExcelItemList;
        public ObservableCollection<IExcelItemModel> ExistedExcelItemList
        {
            get { return _existedExcelItemList; }
            set
            {
                _existedExcelItemList = value;
                RaisePropertyChanged();
            }
        }

        private string _header;
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                RaisePropertyChanged();
            }
        }
        public void SetupCommand()
        {
            //ExportCurrentLayerCommand = new RelayCommand<string>((p) =>
            //{
            //    return true;
            //}, (p) =>
            //{
            //    ExportCurrentLayer();
            //}

            //);
        }
        private List<IExcelItemModel> LoadExistedExcelItemList()
        {
            var itemList = new List<IExcelItemModel>();
            if (!Directory.Exists(ExcelFileFolderPath))
                return itemList;
            using (var stream = File.Open(Path.Combine(ExcelFileFolderPath, "Financial Sample.xlsx"), FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Choose one of either 1 or 2:

                    // 1. Use the reader methods
                    do
                    {
                        while (reader.Read())
                        {
                            // reader.GetDouble(0);
                        }
                    } while (reader.NextResult());

                    // 2. Use the AsDataSet extension method
                    var result = reader.AsDataSet();
                    for (int i = 1; i < result.Tables[0].Rows.Count; i++)
                    {

                        var newItem = new ExcelItemModel()
                        {
                            Name = result.Tables[0].Rows[i]["Column1"].ToString(),
                            Description = result.Tables[0].Rows[i]["Column0"].ToString()
                        };
                        itemList.Add(newItem);
                    }
                    // The result of each spreadsheet is in result.Tables
                }
            }
            return itemList;
        }

    }
}
