using adrilight_shared.View.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;

namespace adrilight_shared.Services
{
    public interface IDialogService
    {
        //void ShowDialog(string name, Action<string> callback);
        void ShowDialog<TViewModel>(Action<string> callback, TViewModel viewmodel);
        void RegisterDialog<Tview, TViewModel>();
    }
    public class DialogService : IDialogService
    {
        static Dictionary<Type, Type> _maping = new Dictionary<Type, Type>();
        public void RegisterDialog<Tview, TViewModel>()
        {
            _maping.Add(typeof(TViewModel), typeof(Tview));
        }

        private static void ShowDialogInternal<TViewModel>(Type type, Action<string> callback, TViewModel viewmodel)
        {
            var dialog = new DialogWindow();
            EventHandler closeEventHandler = null;
            closeEventHandler = (s, e) =>
            {
                callback(dialog.DialogResult.ToString());
                dialog.Closed -= closeEventHandler;
            };
            dialog.Closed += closeEventHandler;
            dialog.Content = Activator.CreateInstance(type);
            (dialog as FrameworkElement).DataContext = viewmodel;
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }
        public void ShowDialog<TViewModel>(Action<string> callback, TViewModel viewmodel)
        {
            var type = _maping[typeof(TViewModel)];
            ShowDialogInternal(type, callback, viewmodel);
        }


    }
}
