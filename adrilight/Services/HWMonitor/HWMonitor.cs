using adrilight.View;
using adrilight.ViewModel;
using adrilight_shared.Enums;
using adrilight_shared.Models.ControlMode.Mode;
using adrilight_shared.Models.ControlMode.ModeParameters;
using adrilight_shared.Models.Device.Zone;
using adrilight_shared.Settings;
using GalaSoft.MvvmLight;
using LibreHardwareMonitor.Hardware;
using LiveCharts;
using LiveCharts.Defaults;
using MathNet.Numerics.Statistics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace adrilight.Util
{
    internal class HWMonitor : ViewModelBase
    {

        public HWMonitor(IGeneralSettings generalSettings, MainViewViewModel mainViewViewModel)
        {

            MainViewViewModel = mainViewViewModel ?? throw new ArgumentNullException(nameof(mainViewViewModel));
            GeneralSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
            MainViewViewModel.AvailableDevices.CollectionChanged += (_, __) => DeviceCollectionChanged();
            RefreshHWState();

        }
        IComputer thisComputer { get; set; }
        private LibreHardwareMonitor.Hardware.Computer computer { get; set; }
        private void DeviceCollectionChanged()
        {
            if (availableFan != null)
            {
                lock (availableFan)
                {
                    availableFan = GetAvailableFans();
                }
            }

        }

        //DependencyInjection//
        private UpdateVisitor updateVisitor = new UpdateVisitor();
        private IGeneralSettings GeneralSettings { get; }

        private MainViewViewModel MainViewViewModel { get; }
        private Computer displayHWInfo { get; set; }
        private double _lastLecture;
        private List<FanMotor> availableFan { get; set; }
        public bool IsRunning { get; private set; } = false;
        private CancellationTokenSource _cancellationTokenSource;
        private void RefreshHWState()
        {
            var isRunning = _cancellationTokenSource != null && IsRunning;
            var shouldBeRunning = true;
            if (isRunning && !shouldBeRunning)
            {
                //stop it!
                Log.Information("stopping the HWMonitor");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }


            else if (!isRunning && shouldBeRunning)
            {
                //start it
                Init();
                Log.Information("Starting the HWMonitor");
                _cancellationTokenSource = new CancellationTokenSource();
                var thread = new Thread(() => Run(_cancellationTokenSource.Token)) {
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal,
                    Name = "HWMonitor"
                };
                thread.Start();
            }
        }
        private List<FanMotor> GetAvailableFans()
        {
            var availableFan = new List<FanMotor>();
            foreach (var device in MainViewViewModel.AvailableDevices.Where(d => d.AvailablePWMDevices != null))
            {
                var pwmOutputs = device.AvailablePWMOutputs.ToList();
                foreach (var pwmOutput in pwmOutputs)
                {
                    var zones = pwmOutput.SlaveDevice.ControlableZones.ToList();
                    foreach (var zone in zones)
                    {
                        var fan = zone as FanMotor;
                        availableFan.Add(fan);
                    }
                }
            }
            return availableFan;
        }

        public void Run(CancellationToken token)//static color creator
        {
            IsRunning = true;

            Log.Information("HWMonitor is Running");

            try
            {
                availableFan = GetAvailableFans();
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
                    Log.Information(hardware.HardwareType.ToString() + " " + hardware.Name);
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
                                Log.Information(sensor.SensorType.ToString() + " " + sensor.Name);
                            }
                            if (sensor.SensorType == SensorType.Fan) // fan speed sensors
                            {
                                fanSpeedSensors.Add(sensor);
                                Log.Information(sensor.SensorType.ToString() + " " + sensor.Name);
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
                            if (sensor.Value.HasValue)
                                speeds.Add((double)sensor.Value);

                        }
                    }
                    else
                    {
                        speeds.Add(80d);
                    }
                    var medianSpeed = speeds.Median();

                    lock (availableFan)
                    {
                        foreach (var fan in availableFan)
                        {
                            if (fan.CurrentActiveControlMode == null)
                                fan.CurrentActiveControlMode = fan.AvailableControlMode.First();
                            if (fan.LineValues == null)
                            {
                                fan.LineValues = new ChartValues<ObservableValue>
                      {
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80),
                        new ObservableValue(80)};
                            }
                            var currentControlMode = fan.CurrentActiveControlMode as PWMMode;
                            int currentSpeed = 80;
                            if (currentControlMode.BasedOn == PWMModeEnum.auto)
                            {
                                currentSpeed = (int)medianSpeed;
                            }
                            else if (currentControlMode.BasedOn == PWMModeEnum.manual)
                            {
                                currentSpeed = (currentControlMode.SpeedParameter as SliderParameter).Value;
                            }
                            if (MainViewViewModel.IsLiveViewOpen && MainViewViewModel.IsAppActivated)
                            {
                                fan.LineValues.Add(new ObservableValue(currentSpeed));
                                fan.LineValues.RemoveAt(0);
                            }
                            fan.CurrentPWMValue = currentSpeed;
                        }
                    }


                    computer.Accept(updateVisitor);
                    Thread.Sleep(1000);
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Error(ex, "OperationCanceledException catched");

                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception catched.");

                Thread.Sleep(500);
            }
            finally
            {

                computer.Close();
                Log.Information("Stopped HW Monitoring!!!");
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