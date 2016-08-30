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
    /// Interaction logic for FileTransferView.xaml
    /// </summary>
    public partial class FileTransferView : UserControl, IView<FileTransferViewModel>
    {
        private FileTransferViewModel _fileTransferViewModel;

        public FileTransferView()
        {
            InitializeComponent();
        }

        public FileTransferViewModel ViewModel
        {
            get { return _fileTransferViewModel; }
            set { _fileTransferViewModel = value; }
        }
    }
}
