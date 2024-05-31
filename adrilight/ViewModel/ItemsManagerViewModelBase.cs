using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace adrilight.ViewModel
{
    public abstract class ItemsManagerViewModelBase : ViewModelBase, IDisposable
    {
        public virtual void LoadData()
        {

        }
        public virtual void Dispose()
        {

        }
    }
}
