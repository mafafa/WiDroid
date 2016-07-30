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
using System.Windows.Shapes;

using BasicWPF.MVVM;
using WiDroid.ViewModels;

namespace WiDroid.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        private IMainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public IMainViewModel ViewModel
        {
            get { return _mainViewModel; }
            set { _mainViewModel = value; }
        }
    }
}
