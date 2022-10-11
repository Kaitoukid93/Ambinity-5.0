using Microsoft.Win32;
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