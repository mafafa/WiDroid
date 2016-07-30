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
    public class MainViewModel : IMainViewModel
    {
        #region Fields

        private IViewModel _currentViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public MainViewModel()
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

        private void NavigateToSettings(object param)
        {
            FlowManager.Instance.ChangePage<SettingsViewModel>();
        }

        #endregion

        #region Properties

        public IViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand NavigateToSettingsCommand
        {
            get { return new RelayCommand(NavigateToSettings); }
        }

        #endregion
    }
}
