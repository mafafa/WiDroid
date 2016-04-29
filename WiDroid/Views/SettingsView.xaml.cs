using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using BasicWPF.MVVM;
using WiDroid.ViewModels;

namespace WiDroid.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl, IView<SettingsViewModel>
    {
        private SettingsViewModel _settingViewModel; 

        public SettingsView()
        {
            InitializeComponent();
        }

        public SettingsViewModel ViewModel
        {
            get { return _settingViewModel; }
            set { _settingViewModel = value; }
        }
    }
}
