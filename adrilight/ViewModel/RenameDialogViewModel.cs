using adrilight_shared.Models;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class RenameDialogViewModel
    {
        public RenameDialogViewModel(string header, IGenericCollectionItem objectToRename)
        {
            CurrentRenameHeader = header;
            CurrentRenamingAceptCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                objectToRename.Name = CurrentRenamingContent;
            });
        }

        public string CurrentRenameHeader { get; set; }
        public ICommand CurrentRenamingAceptCommand { get; set; }
        public string CurrentRenamingContent { get; set; }
    }
}
