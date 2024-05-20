using NonInvasiveKeyboardHookLibrary;

namespace adrilight_shared.Models.KeyboardHook
{
    /// <summary>
    /// This static class turns the non-invasive KeyboardHookManager into a singleton, accessible by all modules
    /// </summary>
    public class KeyboardHookManagerSingleton
    {
        public KeyboardHookManagerSingleton()
        {
            Instance = new KeyboardHookManager();
        }
        public KeyboardHookManager Instance { get; set; }
    }
}