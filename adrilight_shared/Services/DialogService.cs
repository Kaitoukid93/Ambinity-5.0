using adrilight_shared.View.Dialogs;
using System;
using System.Collections.Generic;

namespace adrilight_shared.Services
{
    public interface IDialogService
    {
        //void ShowDialog(string name, Action<string> callback);
        void ShowDialog<ViewModel>(Action<string> callback);
        void RegisterDialog<Tview, TViewModel>();
    }
    public class DialogService : IDialogService
    {
        static Dictionary<Type, Type> _maping = new Dictionary<Type, Type>();
        public void RegisterDialog<Tview, TViewModel>()
        {
            _maping.Add(typeof(TViewModel), typeof(Tview));
        }

        //public void ShowDialog(string name, Action<string> callback)
        //{
        //    ShowDialogInternal(name, callback);
        //}
        private static void ShowDialogInternal(Type type, Action<string> callback)
        {
            var dialog = new DialogWindow();
            dialog.Content = Activator.CreateInstance(type);
            dialog.ShowDialog();
        }
        public void ShowDialog<TViewModel>(Action<string> callback)
        {
            var type = _maping[typeof(TViewModel)];
            ShowDialogInternal(type, callback);
        }


    }
}
