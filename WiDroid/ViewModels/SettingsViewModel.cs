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

        public enum SettingsState { Default, Changing, Changed }

        private SettingsState _settingsState;
        private int _discoveryPort;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public SettingsViewModel()
        {

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
            Properties.Settings.Default.DiscoveryPort = DiscoveryPort;
            Properties.Settings.Default.Save();

            SettingsCurrentState = SettingsState.Changed;
        }

        private void RevertSettings(object param)
        {
            // We use the fields here since we don't want to set SettingsHaveChanged to true
            _discoveryPort = Properties.Settings.Default.DiscoveryPort;
            RaisePropertyChanged("DiscoveryPort");

            SettingsCurrentState = SettingsState.Changed;
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

        public SettingsState SettingsCurrentState
        {
            get { return _settingsState; }
            set
            {
                if (_settingsState != value)
                {
                    _settingsState = value;
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

                    SettingsCurrentState = SettingsState.Changing;
                }
            }
        }

        #endregion
    }
}
