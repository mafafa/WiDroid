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
        #region Fields

        private int _discoveryPort;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

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

        #region Properties

        public int DiscoveryPort
        {
            get { return _discoveryPort; }
            set
            {
                if (_discoveryPort != value)
                {
                    _discoveryPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
    }
}
