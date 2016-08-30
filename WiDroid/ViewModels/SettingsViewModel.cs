using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using BasicWPF;
using BasicWPF.MVVM;


namespace WiDroid.ViewModels
{
    public class SettingsViewModel : IViewModel
    {
        #region Fields

        private bool _settingsHaveChanged;
        private int _discoveryPort;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public SettingsViewModel()
        {
            _settingsHaveChanged = false;
        }

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

        private void SaveSettings(object param)
        {
            
        }

        private void RevertSettings(object param)
        {

        }

        #endregion

        #region Properties

        public ICommand SaveSettingsCommand
        {
            get { return new RelayCommand(SaveSettings); }
        }

        public ICommand RevertSettingsCommand
        {
            get { return new RelayCommand(RevertSettings); }
        }

        public bool SettingsHaveChanged
        {
            get { return _settingsHaveChanged; }
            set
            {
                if (_settingsHaveChanged != value)
                {
                    _settingsHaveChanged = value;
                    RaisePropertyChanged();
                }
            }
        }

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
