using System;
using System.Windows.Media.Imaging;
using adrilight.Util;
using System.Threading;
using NLog;
using adrilight.Spots;
using System.Runtime.InteropServices;
using System.Windows;
using adrilight.ViewModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Forms;
using NonInvasiveKeyboardHookLibrary;

namespace adrilight
{
    /// <summary>
    /// This static class turns the non-invasive KeyboardHookManager into a singleton, accessible by all modules
    /// </summary>
    public static class HotKeyManager
    {
        private static KeyboardHookManager _keyboardHookManager;

        public static KeyboardHookManager Instance => _keyboardHookManager ?? (_keyboardHookManager = new KeyboardHookManager());
    }
    //internal class HotKeyManager : ViewModelBase, IHotKeyManager
    //{


    //    private readonly NLog.ILogger _log = LogManager.GetCurrentClassLogger();


    //    /////////////////////////////////////////////////////////////
    //    ////A bunch of DLL Imports to set a low level keyboard hook
    //    /////////////////////////////////////////////////////////////
    //    //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    //private static extern IntPtr SetWindowsHookEx(int idHook,
    //    //    LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    //    //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    //[return: MarshalAs(UnmanagedType.Bool)]
    //    //private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    //    //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    //private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
    //    //    IntPtr wParam, IntPtr lParam);

    //    //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    //private static extern IntPtr GetModuleHandle(string lpModuleName);

    //    //////////////////////////////////////////////////////////////////
    //    ////Some constants to make handling our hook code easier to read
    //    //////////////////////////////////////////////////////////////////
    //    //private const int WH_KEYBOARD_LL = 13;                    //Type of Hook - Low Level Keyboard
    //    //private const int WM_KEYDOWN = 0x0100;                    //Value passed on KeyDown
    //    //private const int WM_KEYUP = 0x0101;                      //Value passed on KeyUp
    //    //private static LowLevelKeyboardProc _proc = HookCallback; //The function called when a key is pressed
    //    //private static IntPtr _hookID = IntPtr.Zero;
    //    //private static bool CONTROL_DOWN = false;                 //Bool to use as a flag for control key
    //    public HotKeyManager(MainViewViewModel mainViewModel)
    //    {
    //       MainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel)); 

    //        RefreshHookingState();
    //        _log.Info($"Hot Key Manager Created");

    //    }
    //    //Dependency Injection//
    //    private IGeneralSettings GeneralSettings { get; }
    //    private static MainViewViewModel MainViewModel { get; set; }

    //    public bool IsRunning { get; private set; } = false;
    //    private CancellationTokenSource _cancellationTokenSource;



    //    private void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)// get hot key settings value from user input
    //    {
    //        switch (e.PropertyName)
    //        {

    //            case nameof(GeneralSettings.ShaderCanvasWidth):
    //            case nameof(GeneralSettings.ShaderCanvasHeight):
    //            case nameof(GeneralSettings.ShaderX):
    //            case nameof(GeneralSettings.ShaderY):
    //            case nameof(GeneralSettings.SelectedShader):

    //                RefreshHookingState();
    //                break;
    //        }
    //    }


    //    //private IntPtr _windowHandle;
    //    //private HwndSource _source;
    //    //private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    //    //private static IntPtr SetHook(LowLevelKeyboardProc proc)
    //    //{
    //    //    using (Process curProcess = Process.GetCurrentProcess())
    //    //    using (ProcessModule curModule = curProcess.MainModule)
    //    //    {
    //    //        return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
    //    //            GetModuleHandle(curModule.ModuleName), 0);
    //    //    }
    //    //}






    //    private void RefreshHookingState()
    //    {
    //        var keyboardHookManager = new KeyboardHookManager();
    //        //keyboardHookManager.Start();
    //        // Register virtual key code 0x60 = NumPad0
    //        keyboardHookManager.RegisterHotkey(120, () =>
    //        {
    //            Console.WriteLine("F9 detected");
    //        });

    //        // Modifiers are supported too
    //        keyboardHookManager.RegisterHotkey(NonInvasiveKeyboardHookLibrary.ModifierKeys.Control, 120, () =>
    //        {
    //            Console.WriteLine("Ctrl+F9 detected");
    //        });

    //        // Multiple modifiers can be specified using the bitwise OR operation
    //        keyboardHookManager.RegisterHotkey(NonInvasiveKeyboardHookLibrary.ModifierKeys.Control | NonInvasiveKeyboardHookLibrary.ModifierKeys.Alt, 120, () =>
    //        {
    //            Console.WriteLine("Ctrl+Alt+F9 detected");
    //        });

    //        // Or as an array of modifiers

    //    }



    //    //private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    //    //{
    //    //    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) //A Key was pressed down
    //    //    {
    //    //        int vkCode = Marshal.ReadInt32(lParam);           //Get the keycode
    //    //        string theKey = ((Keys)vkCode).ToString();        //Name of the key
    //    //        Debug.WriteLine(theKey);                            //Display the name of the key
    //    //        if (theKey.Contains("ControlKey"))                //If they pressed control
    //    //        {
    //    //            CONTROL_DOWN = true;                          //Flag control as down
    //    //        }
    //    //        else if (CONTROL_DOWN)           //If they held CTRL and pressed B
    //    //        {
    //    //            Debug.WriteLine("\n***HOTKEY PRESSED***");  //Our hotkey was pressed 
    //    //            MainViewModel.HotKeyDetected("Control", vkCode);// send to viewmodel the combination
    //    //        }
    //    //        else if (theKey == "Escape")                      //If they press escape
    //    //        {
    //    //            UnhookWindowsHookEx(_hookID);                 //Release our hook
    //    //            Environment.Exit(0);                          //Exit our program
    //    //        }
    //    //    }
    //    //    else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) //KeyUP
    //    //    {
    //    //        int vkCode = Marshal.ReadInt32(lParam);        //Get Keycode
    //    //        string theKey = ((Keys)vkCode).ToString();     //Get Key name
    //    //        if (theKey.Contains("ControlKey"))             //If they let go of control
    //    //        {
    //    //            CONTROL_DOWN = false;                      //Unflag control
    //    //        }
    //    //    }
    //    //    return CallNextHookEx(_hookID, nCode, wParam, lParam); //Call the next hook
    //    //}


    //    public void Run(CancellationToken token)

    //    {

    //        //if (IsRunning) throw new Exception(" Shader Effects is already running!");

    //        //IsRunning = true;

    //        //_log.Debug("Started Shader Effects Color.");


    //        //try
    //        //{
    //        //    using ReadWriteTexture2D<Bgra32, Float4> texture = Gpu.Default.AllocateReadWriteTexture2D<Bgra32, Float4>(240, 135);

    //        //    int frame = 0;

    //        //    Frame = null;


    //        //    while (!token.IsCancellationRequested)
    //        //    {



    //        //        var frameTime = Stopwatch.StartNew();

    //        //        // For each frame

    //        //        float timestamp = 1 / 60f * frame;

    //        //                // Run the shader
    //        //               switch(GeneralSettings.SelectedShader)
    //        //        {
    //        //            case "Gooey":
    //        //                Gpu.Default.ForEach(texture, new Gooey(timestamp));
    //        //                break;
    //        //            case "Fluid":
    //        //                Gpu.Default.ForEach(texture, new Fluid(timestamp));
    //        //                break;
    //        //            case "Plasma":
    //        //                Gpu.Default.ForEach(texture, new Plasma(timestamp));
    //        //                break;
    //        //            case "Falling":
    //        //                Gpu.Default.ForEach(texture, new Falling(timestamp));
    //        //                break;
    //        //            case "MetaBalls":
    //        //                Gpu.Default.ForEach(texture, new MetaBalls(timestamp));

    //        //                break;
    //        //            case "Pixel Rainbow":
    //        //                Gpu.Default.ForEach(texture, new PixelRainbow(timestamp));

    //        //                break;



    //        //        }


    //        //        // Copy the rendered frame to a readback texture that can be accessed by the CPU.
    //        //        // The rendered texture can only be accessed by the GPU, so we can't read from it.
    //        //        var colorArray = new Bgra32[texture.Width * texture.Height];
    //        //        var bitmapSpan = new Span<Bgra32>(colorArray);
    //        //        // Access buffer.View here and do all your work with the frame data

    //        //        texture.CopyTo(bitmapSpan);

    //        //        var bytes = MemoryMarshal.Cast<Bgra32, byte>(bitmapSpan);
    //        //        Frame = bytes.ToArray();    
    //        //        RaisePropertyChanged(nameof(Frame));
    //        //        int minFrameTimeInMs = 1000 / GeneralSettings.LimitFps;
    //        //        var elapsedMs = (int)frameTime.ElapsedMilliseconds;
    //        //        if (elapsedMs < minFrameTimeInMs)
    //        //        {
    //        //            Thread.Sleep(minFrameTimeInMs - elapsedMs);
    //        //        }
    //        //        frame++;

    //        //    }
    //        //}
    //        //catch (OperationCanceledException)
    //        //{
    //        //    _log.Debug("OperationCanceledException catched. returning.");


    //        //}
    //        //catch (Exception ex)
    //        //{
    //        //    _log.Debug(ex, "Exception catched.");

    //        //    //allow the system some time to recover
    //        //    Thread.Sleep(500);
    //        //}
    //        //finally
    //        //{

    //        //    _log.Debug("Stopped Rainbow Color Creator.");
    //        //    IsRunning = false;
    //        //}


    //    }




    //}
}
