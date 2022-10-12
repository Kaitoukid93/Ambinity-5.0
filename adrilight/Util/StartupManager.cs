using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Security.Principal;

public class StartUpManager
{
    private const string ApplicationName = "adrilight";

    public static void AddApplicationToCurrentUserStartup()
    {
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            string folder = Path.GetDirectoryName(location);
            key.SetValue(ApplicationName, folder + "\\" + "adrilight.exe");
        }
    }

    public static void AddApplicationToAllUserStartup()
    {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            string folder = Path.GetDirectoryName(location);
            key.SetValue(ApplicationName, folder + "\\" + "adrilight.exe");
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
    public static void AddApplicationToTaskScheduler()
    {
        TaskService ts = new TaskService();
        TaskDefinition td = ts.NewTask();
        td.Principal.RunLevel = TaskRunLevel.Highest;
        //td.Triggers.AddNew(TaskTriggerType.Logon);          
        td.Triggers.AddNew(TaskTriggerType.Logon);    // 
        string program_path = "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\""; // you can have it dynamic
                                                                                                        //even of user choice giving an interface in win-form or wpf application

        td.Actions.Add(new ExecAction(program_path, null));
        ts.RootFolder.RegisterTaskDefinition("Ambinity Service", td);
    }
    //public static void DeleteApplicationFromTaskScheduler(string taskName)
    //{

    //    // Retrieve the task, change the trigger and re-register it.
    //    // A taskName by itself assumes the root folder (e.g. "MyTask")
    //    // A taskName can include folders (e.g. "MyFolder\MySubFolder\MyTask")
    //    TaskService ts = new TaskService();
    //    Task t = ts.GetTask(taskName);
    //        if (t == null) return;
    //        t.Definition.Triggers[0](0).StartBoundary = DateTime.Today + TimeSpan.FromDays(7);
    //        t.RegisterChanges();

    //        // Check to make sure account privileges allow task deletion
    //        var identity = WindowsIdentity.GetCurrent();
    //        var principal = new WindowsPrincipal(identity);
    //        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
    //            throw new Exception($"Cannot delete task with your current identity '{identity.Name}' permissions level." +
    //            "You likely need to run this application 'as administrator' even if you are using an administrator account.");

    //        // Remove the task we just created
    //        ts.RootFolder.DeleteTask(taskName);
        
    //}
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