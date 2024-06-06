using adrilight_shared.Models.RelayCommand;
using adrilight_shared.Models.Store;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace adrilight.ViewModel.AdrilightStore
{
    public class AdrilightStoreItemDetailViewModel : ViewModelBase
    {
        public event Action<OnlineItemModel> ItemDownloadButtonClicked;
        public AdrilightStoreItemDetailViewModel(AdrilightStoreSFTPClient client)
        {
            _client = client;
            CommandSetup();
        }
        private OnlineItemModel _item;
        public OnlineItemModel Item {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<BitmapImage> _screenShots;
        public ObservableCollection<BitmapImage> ScreenShots {
            get
            {
                return _screenShots;
            }
            set
            {
                _screenShots = value;
                RaisePropertyChanged();
            }
        }
        private string _markDownDescription;
        public string MarkDownDescription {
            get
            {
                return _markDownDescription;
            }
            set
            {
                _markDownDescription = value;
                RaisePropertyChanged();
            }
        }
        private AdrilightStoreSFTPClient _client;
        public async Task Init(OnlineItemModel item, IProgress<int> progress = null)
        {
            Item = item;
            progress.Report(20);
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ScreenShots = new ObservableCollection<BitmapImage>();

            });
            var availableScreenShots = await _client.GetItemScreenShots(item);
            progress.Report(40);
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {

                foreach (var img in availableScreenShots)
                {
                    ScreenShots.Add(img);
                }
            });
            progress.Report(80);
            MarkDownDescription = await _client.GetItemDescription(item);
            progress.Report(100);
        }
        private void CommandSetup()
        {
            DownloadButtonClickCommand = new RelayCommand<string>((p) =>
            {
                return p != null;
            }, (p) =>
            {
                ItemDownloadButtonClicked?.Invoke(Item);

            });
        }
        public ICommand DownloadButtonClickCommand { get; set; }
    }
}
