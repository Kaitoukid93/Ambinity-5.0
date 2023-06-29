using adrilight.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {

        public MainView()
        {
            InitializeComponent();
            // ViewModel = new MainViewViewModel();
            // this.DataContext = ViewModel;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.9;
            this.Width = SystemParameters.PrimaryScreenWidth * 0.8;
            // new Thread(Observe).Start();

            noticon.Init();

            var view = DataContext as MainViewViewModel;
            if (view.GeneralSettings.HotkeyEnable)
            {
                KeyboardHookManagerSingleton.Instance.Start();
                view.Register();
            }
        }
        private void Observe()
        {
            new PreferenceChangedObserver().Run();
        }
        internal sealed class PreferenceChangedObserver
        {
            private readonly string _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "adrilight\\log.txt");

            private BindingFlags _flagsStatic = BindingFlags.NonPublic | BindingFlags.Static;
            private BindingFlags _flagsInstance = BindingFlags.NonPublic | BindingFlags.Instance;

            public void Run() => CheckSystemEventsHandlersForFreeze();

            private void CheckSystemEventsHandlersForFreeze()
            {
                while (true)
                {
                    try
                    {
                        foreach (var info in GetPossiblyBlockingEventHandlers())
                        {
                            var msg = $"SystemEvents handler '{info.EventHandlerDelegate.Method.DeclaringType}.{info.EventHandlerDelegate.Method.Name}' could freeze app due to wrong thread. ThreadId: {info.Thread.ManagedThreadId}, IsThreadPoolThread:{info.Thread.IsThreadPoolThread}, IsAlive:{info.Thread.IsAlive}, ThreadName:{info.Thread.Name}{Environment.NewLine}{info.StackTrace}{Environment.NewLine}";
                            File.AppendAllText(_logFilePath, DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + $": {msg}{Environment.NewLine}");
                        }
                    }
                    catch { }
                }
            }

            private IEnumerable<EventHandlerInfo> GetPossiblyBlockingEventHandlers()
            {
                var handlers = typeof(SystemEvents).GetField("_handlers", _flagsStatic).GetValue(null);

                if (!(handlers?.GetType().GetProperty("Values").GetValue(handlers) is IEnumerable handlersValues))
                    yield break;

                foreach (var systemInvokeInfo in handlersValues.Cast<IEnumerable>().SelectMany(x => x.OfType<object>()).ToList())
                {
                    var syncContext = systemInvokeInfo.GetType().GetField("_syncContext", _flagsInstance).GetValue(systemInvokeInfo);

                    //Make sure its the problematic type
                    if (!(syncContext is WindowsFormsSynchronizationContext wfsc))
                        continue;

                    //Get the thread
                    var threadRef = (WeakReference)syncContext.GetType().GetField("destinationThreadRef", _flagsInstance).GetValue(syncContext);
                    if (!threadRef.IsAlive)
                        continue;

                    var thread = (Thread)threadRef.Target;
                    if (thread.ManagedThreadId == 1) //UI thread
                        continue;

                    if (thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
                        continue;

                    //Get the event delegate
                    var eventHandlerDelegate = (Delegate)systemInvokeInfo.GetType().GetField("_delegate", _flagsInstance).GetValue(systemInvokeInfo);

                    //Get the threads call stack
                    string callStack = string.Empty;
                    try
                    {
                        if (thread.IsAlive)
                            callStack = GetStackTrace(thread)?.ToString().Trim();
                    }
                    catch { }

                    yield return new EventHandlerInfo {
                        Thread = thread,
                        EventHandlerDelegate = eventHandlerDelegate,
                        StackTrace = callStack,
                    };
                }
            }

            private static StackTrace GetStackTrace(Thread targetThread)
            {
                using (ManualResetEvent fallbackThreadReady = new ManualResetEvent(false), exitedSafely = new ManualResetEvent(false))
                {
                    Thread fallbackThread = new Thread(delegate () {
                        fallbackThreadReady.Set();
                        while (!exitedSafely.WaitOne(200))
                        {
                            try
                            {
                                targetThread.Resume();
                            }
                            catch (Exception) {/*Whatever happens, do never stop to resume the target-thread regularly until the main-thread has exited safely.*/}
                        }
                    });
                    fallbackThread.Name = "GetStackFallbackThread";
                    try
                    {
                        fallbackThread.Start();
                        fallbackThreadReady.WaitOne();
                        //From here, you have about 200ms to get the stack-trace.
                        targetThread.Suspend();
                        StackTrace trace = null;
                        try
                        {
                            trace = new StackTrace(targetThread, true);
                        }
                        catch (ThreadStateException) { }
                        try
                        {
                            targetThread.Resume();
                        }
                        catch (ThreadStateException) {/*Thread is running again already*/}
                        return trace;
                    }
                    finally
                    {
                        //Just signal the backup-thread to stop.
                        exitedSafely.Set();
                        //Join the thread to avoid disposing "exited safely" too early. And also make sure that no leftover threads are cluttering iis by accident.
                        fallbackThread.Join();
                    }
                }
            }

            private class EventHandlerInfo
            {
                public Delegate EventHandlerDelegate { get; set; }
                public Thread Thread { get; set; }
                public string StackTrace { get; set; }
            }
        }
        //protected override void OnClosed(EventArgs e)
        //{
        //    _source.RemoveHook(HwndHook);
        //    UnregisterHotKey(_windowHandle, HOTKEY_ID);
        //    base.OnClosed(e);
        //}
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            NonClientAreaContent = new NonClientAreaContent();

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainViewViewModel;
            vm.BackToDashboard();
            e.Cancel = true;
            // Hide Window instead
            this.Visibility = Visibility.Collapsed;
        }

        private void Window_Closed(object sender, EventArgs e)
        {


        }

        private void OpenDashboard(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
        }
    }
}
