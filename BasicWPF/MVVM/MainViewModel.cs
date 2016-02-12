using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BasicWPF.MVVM
{
    public class MainViewModel : IViewModel
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

        #endregion
    }
}
