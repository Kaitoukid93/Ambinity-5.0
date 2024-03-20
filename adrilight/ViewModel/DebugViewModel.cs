using adrilight.Helpers;
using adrilight.Ticker;
using adrilight.View;
using adrilight_shared.Enums;
using adrilight_shared.Helpers;
using adrilight_shared.Models;
using adrilight_shared.Models.ControlMode;
using adrilight_shared.Models.Device;
using adrilight_shared.Models.Device.Controller;
using adrilight_shared.Models.Device.Output;
using adrilight_shared.Models.Device.SlaveDevice;
using adrilight_shared.Models.Lighting;
using adrilight_shared.Models.Stores;
using adrilight_shared.Services;
using adrilight_shared.View.NonClientAreaContent;
using adrilight_shared.ViewModel;
using CSCore.CoreAudioAPI;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

namespace adrilight.ViewModel
{
    public class DebugViewModel : ViewModelBase
    {
        #region Construct
        
        public DebugViewModel()
        {
           
            CommandSetup();
        }
        #endregion

        #region Properties
     
        private string _terminalCommandString;
        public string TerminalCommandString {
            get
            {
                return _terminalCommandString;
            }
            set
            {
                _terminalCommandString = value;
                RaisePropertyChanged();
            }
        }

        #endregion
       

        #region Methods
        private void CommandSetup()
        {

            SendTerminalCommand = new RelayCommand<string>((p) =>
            {
                return true;
            },  (p) =>
            {
                 SendTerminal();

            });
          
        }
        
        #endregion


        #region Commands
        public ICommand SendTerminalCommand { get; set; }
       
        #endregion


       private void SendTerminal() {

        }


    }
}
