using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using BasicWPF.MVVM;

namespace WiDroid.ViewModels
{
    public class SettingsViewModel : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Protected Methods

        protected void RaisePropertyChanged([CallerMemberName]string propertName = "")
        {
            var temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertName));
            }
        }

        #endregion
    }
}
