using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;
using Castle.Core.Logging;
using NLog;
using adrilight.ViewModel;
using System.Diagnostics;
using adrilight.Spots;
using LibreHardwareMonitor.Hardware;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using MathNet.Numerics.Statistics;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows;
using adrilight.View;

namespace adrilight.Util
{
    internal class HWMonitor : ViewModelBase, IHWMonitor
    {


        private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();


        public HWMonitor(IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {

            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

            RefreshHWState();
            _log.Info($"Hardware Monitor Created");

        }
        IComputer thisComputer { get; set; }



        private LibreHardwareMonitor.Hardware.Computer computer { get; set; }
        private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    case nameof(OutputSettings.OutputIsEnabled):
            //    case nameof(OutputSettings.OutputSelectedMode):

            //        RefreshColorState();
            //        break;
            //    case nameof(OutputSettings.OutputStaticColor):
            //    case nameof(OutputSettings.OutputSelectedGradient):
            //        SolidColorChanged();
            //        break;

            //}
        }

        //DependencyInjection//
        private UpdateVisitor updateVisitor = new UpdateVisitor();
        private IGeneralSettings GeneralSettings { get; }

        private MainViewViewModel MainViewViewModel { get; }
        private Computer displayHWInfo { get; set; }
        private double _lastLecture;
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;
        private void RefreshHWState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true;
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                _log.Debug("stopping the HWMonitor");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }


            else if (!isRunning && shouldBeRunning)
            {
                //start it
                Init();
                _log.Debug("starting the HWMonitor");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "HWMonitor"
                };
                thread.Start();
            }
        }


        public void Run(CancellationToken token)//static color creator
        {

            if (IsRunning) throw new Exception(" HWMonitor is already running!");

            IsRunning = true;

            _log.Debug("Started HW Monitor.");


            //init new hardware and sensor for dispayHWInfo


            try
            {

                thisComputer = new Computer();
                thisComputer.Processor = new List<IHardware>(); // init cpu list
                thisComputer.MotherBoard = new List<IHardware>(); // init mb list
                thisComputer.Ram = new List<IHardware>(); // init mb list
                thisComputer.GraphicCard = new List<IHardware>(); // init mb list
                var fanControlSensors = new List<ISensor>();
                var fanSpeedSensors = new List<ISensor>();

                computer.Accept(updateVisitor);
                foreach (var hardware in computer.Hardware)
                {

                    if (hardware.HardwareType == HardwareType.Cpu)
                        thisComputer.Processor.Add(hardware);
                    if (hardware.HardwareType == HardwareType.Motherboard)
                        thisComputer.MotherBoard.Add(hardware);
                    if (hardware.HardwareType == HardwareType.Memory)
                        thisComputer.Ram.Add(hardware);
                    if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                        thisComputer.GraphicCard.Add(hardware);
                }
                //add all fancontrol sensor to the list

                if (thisComputer.MotherBoard.Count > 0)// check if we get any motherboard
                {
                    if (thisComputer.MotherBoard[0].SubHardware.Length > 0) // check if any subhardware in motherboard
                    {
                        foreach (var sensor in thisComputer.MotherBoard[0].SubHardware[0].Sensors)
                        {
                            if (sensor.SensorType == SensorType.Control)//speed control sensors
                            {
                                fanControlSensors.Add(sensor);
                            }
                            if (sensor.SensorType == SensorType.Fan) // fan speed sensors
                            {
                                fanSpeedSensors.Add(sensor);
                            }
                        }


                    }
                    else
                    {
                        if (GeneralSettings.HWMonitorAskAgain)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                var dialog = new CommonInfoDialog();
                                //dialog.header.Text = "OpenRGB is disabled"
                                dialog.question.Text = "Không đọc được thông tin Fan Controller , thử khởi chạy lại ứng dụng với quyền Admin";
                                bool? result = dialog.ShowDialog();
                                if (result == true)
                                {
                                    // Enable OpenRGB
                                    GeneralSettings.IsOpenRGBEnabled = true;



                                }
                                if (dialog.askagaincheckbox.IsChecked == true)
                                {
                                    GeneralSettings.HWMonitorAskAgain = false;
                                }
                                else
                                {
                                    GeneralSettings.HWMonitorAskAgain = true;
                                }

                            });
                        }

                    }
                }
                else
                {
                    //there is no fking mainboard here, could be LHM's fault or some shit happened
                    // just tell ya
                    if (GeneralSettings.HWMonitorAskAgain)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            var dialog = new CommonInfoDialog();
                            //dialog.header.Text = "OpenRGB is disabled"
                            dialog.question.Text = "Không đọc được thông tin Motherboard , thử khởi chạy lại ứng dụng với quyền Admin";
                            bool? result = dialog.ShowDialog();
                            if (result == true)
                            {
                                // Enable OpenRGB
                                GeneralSettings.IsOpenRGBEnabled = true;



                            }
                            if (dialog.askagaincheckbox.IsChecked == true)
                            {
                                GeneralSettings.HWMonitorAskAgain = false;
                            }
                            else
                            {
                                GeneralSettings.HWMonitorAskAgain = true;
                            }

                        });
                    }

                }

                if (fanControlSensors.Count <= 0)
                {
                    if (GeneralSettings.HWMonitorAskAgain)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            var dialog = new CommonInfoDialog();
                            //dialog.header.Text = "OpenRGB is disabled"
                            dialog.question.Text = "Fan sẽ chạy với tốc độ mặc định hoặc điều chỉnh bằng tay";
                            bool? result = dialog.ShowDialog();
                            if (result == true)
                            {
                                // Enable OpenRGB
                                GeneralSettings.IsOpenRGBEnabled = true;



                            }
                            if (dialog.askagaincheckbox.IsChecked == true)
                            {
                                GeneralSettings.HWMonitorAskAgain = false;
                            }
                            else
                            {
                                GeneralSettings.HWMonitorAskAgain = true;
                            }

                        });
                    }

                    foreach (var device in MainViewViewModel.AvailableDevices.Where(x => x.DeviceType == "ABFANHUB"))
                    {
                        //find speed control and set
                        //foreach (var output in device.AvailableOutputs)
                        //{
                        //    (output as OutputSettings).SetSpeed(200);
                        //}

                    }
                }

                while (!token.IsCancellationRequested)
                {


                    //Normally, most of the fan header is empty, some motherboard will fail to read RPM and push max speed control to that header
                    // we need to detech which header is empty and if it has speed control of 100, then we will remove that header from equation
                    if (fanSpeedSensors.Count > 0)
                    {
                        for (var i = 0; i < fanSpeedSensors.Count; i++)
                        {
                            if (fanSpeedSensors[i].Value == double.NaN)// this is speed target control but header is empty
                            {
                                fanControlSensors.RemoveAt(i);
                            }

                        }
                    }


                    //get median fan control speed value
                    List<double> speeds = new List<double>();
                    if (fanControlSensors.Count > 0)
                    {
                        foreach (var sensor in fanControlSensors)
                        {
                            speeds.Add((double)sensor.Value);
                        }
                    }
                    else
                    {
                        speeds.Add(80d);
                    }
                    var medianSpeed = speeds.Median();
                    if (MainViewViewModel.IsSplitLightingWindowOpen)
                    {
                        MainViewViewModel.FanControlView[0].Values.Add(new ObservableValue(medianSpeed));
                        MainViewViewModel.FanControlView[0].Values.RemoveAt(0);

                    }
                    // decide if it's necessary to update to the fan
                    //formular is if the fan speed changed more than 5% compare to last fan speed, apply the change

                    //it's time to tell the fan to update the speed
                    /*
                    foreach (var device in MainViewViewModel.AvailableDevices.Where(x => x.DeviceType == "ABFANHUB"))
                    {
                        foreach (var output in device.AvailableOutputs)
                        {
                            var currentOutput = output as OutputSettings;
                            if (Math.Abs((int)(medianSpeed * 255 / 100) - currentOutput.GetSpeed()) > 15)
                            {

                                if (currentOutput.GetCurrentSpeedMode() == SpeedModeEnum.auto)
                                {
                                    var value = ((int)medianSpeed * 255) / 100;
                                    currentOutput.SetSpeed(value);
                                }
                                   
                            }

                        }
                    }
                    */






                    computer.Accept(updateVisitor);



                    Thread.Sleep(1000);
                }
                // update every second








            }
            //motion speed




            catch (OperationCanceledException)
            {
                _log.Debug("OperationCanceledException catched. returning.");

                return;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Exception catched.");

                Thread.Sleep(500);
            }
            finally
            {

                computer.Close();
                _log.Debug("Stopped HW Monitoring!!!");
                IsRunning = false;
            }

        }


        public void Init()
        {
            computer = new LibreHardwareMonitor.Hardware.Computer {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true

            };

            computer.Open();
            computer.Accept(updateVisitor);
            displayHWInfo = new Computer();

        }

        public void Dispose()
        {
            computer.Close();
        }





    }
}