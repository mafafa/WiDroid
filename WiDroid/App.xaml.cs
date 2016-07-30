using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using BasicWPF.MVVM;
using WiDroid.Views;
using WiDroid.ViewModels;

namespace WiDroid
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Show main window
            MainWindow mainWindow = new MainWindow();
            MainViewModel mainViewModel = new MainViewModel();
            mainWindow.DataContext = mainViewModel;
            mainWindow.ViewModel = mainViewModel;

            FlowManager.Instance.AppWindow = mainWindow;

            mainWindow.Show();
        }
    }
}
