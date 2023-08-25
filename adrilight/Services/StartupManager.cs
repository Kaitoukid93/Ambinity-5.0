using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Security.Principal;

public class StartUpManager
{
    private const string ApplicationName = "adrilight";

    public static void AddApplicationToCurrentUserStartup()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.SetValue(ApplicationName, "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
        }
    }

    public static void AddApplicationToAllUserStartup()
    {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.SetValue(ApplicationName, "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
        }
    }

    public static void RemoveApplicationFromCurrentUserStartup()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.DeleteValue(ApplicationName, false);
        }
    }

    public static void RemoveApplicationFromAllUserStartup()
    {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            key.DeleteValue(ApplicationName, false);
        }
    }
    public static void AddApplicationToTaskScheduler(int delaySecond)
    {
        TaskService ts = new TaskService();
        TaskDefinition td = ts.NewTask();
        td.Principal.RunLevel = TaskRunLevel.Highest;
        //td.Triggers.AddNew(TaskTriggerType.Logon);          
        td.Triggers.AddNew(TaskTriggerType.Logon);   // 
        string program_path = "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\""; // you can have it dynamic
        BootTrigger bt = new BootTrigger();
        bt.Delay = TimeSpan.FromSeconds(delaySecond);
        td.Triggers.Add(bt);                                                                                             //even of user choice giving an interface in win-form or wpf application
        td.Actions.Add(new ExecAction(program_path, null));
        ts.RootFolder.RegisterTaskDefinition("Ambinity Service", td);
    }
    public static void RemoveApplicationFromTaskScheduler(string taskName)
    {
        using TaskService ts = new TaskService();
        if (ts.GetTask(taskName) != null)
        {
            ts.RootFolder.DeleteTask(taskName);
        }
    }
    public static bool IsUserAdministrator()
    {
        //bool value to hold our return value
        bool isAdmin;
        try
        {
            //get the currently logged in user
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (UnauthorizedAccessException)
        {
            isAdmin = false;
        }
        catch (Exception)
        {
            isAdmin = false;
        }
        return isAdmin;
    }
}