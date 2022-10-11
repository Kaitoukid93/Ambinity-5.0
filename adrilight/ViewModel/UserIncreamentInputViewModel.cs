using adrilight.Spots;

using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace adrilight.ViewModel
{
    public class UserIncreamentInputViewModel : ViewModelBase
    {
        private int _startIndex;
        public int StartIndex {
            get { return _startIndex; }
            set
            {
                if (_startIndex == value) return;
                _startIndex = value;
                RaisePropertyChanged();
            }
        }
        private int _spacing;
        public int Spacing {
            get { return _spacing; }
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                RaisePropertyChanged();
            }
        }
        private int _startPoint;
        public int StartPoint {
            get { return _startPoint; }
            set
            {
                if (_startPoint == value) return;
                _startPoint = value;
                RaisePropertyChanged();
            }
        }
        public IDeviceSpot[] previewSpots;
        private int _endPoint;
        public int EndPoint {
            get { return _endPoint; }
            set
            {
                if (_endPoint == value) return;
                _endPoint = value;
                RaisePropertyChanged();
            }
        }
        private int _maxSpotsLength;

        public int MaxSpotsLength {
            get { return _maxSpotsLength; }
            set
            {
                if (_maxSpotsLength == value) return;
                _maxSpotsLength = value;
                RaisePropertyChanged();
            }
        }
        private int _spreadNumber;
        public int SpreadNumber {
            get { return _spreadNumber; }
            set
            {
                if (_spreadNumber == value) return;
                _spreadNumber = value;
                RaisePropertyChanged();
            }
        }
        public UserIncreamentInputViewModel(IDeviceSpot[] spots)
        {
            previewSpots = spots;
            MaxSpotsLength = previewSpots.Length-1;
            //Card = device;

            //DeleteCommand = new RelayCommand<string>((p) => {
            //    return true;
            //}, (p) =>
            //{
            //   // some action
            //});

        }

    }
}
