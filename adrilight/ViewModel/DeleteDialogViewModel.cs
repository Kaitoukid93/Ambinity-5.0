using adrilight_shared.Models;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class DeleteDialogViewModel
    {
        public DeleteDialogViewModel(string header, DataCollection collection)
        {
            CurrentDeleteHeader = header;
            CurrentDeleteAceptCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                collection.RemoveItems();
            });
        }
        public string CurrentDeleteHeader { get; set; }
        public ICommand CurrentDeleteAceptCommand { get; set; }
    }
}
