using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;

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
            using (UnityContainer container = new UnityContainer())
            {
                // Load settings
                int discoveryPort = WiDroid.Properties.Settings.Default.DiscoveryPort;

                // Create Unity container and load all types
                container.RegisterType<MainViewModel>();
                container.RegisterType<SettingsViewModel>(new InjectionProperty("DiscoveryPort", discoveryPort), 
                    new InjectionProperty("SettingsCurrentState", SettingsViewModel.SettingsState.Default));
                container.RegisterType<FileTransferViewModel>();

                // Start client discovery server
                ClientDiscoveryServer.ClientDiscoveryServer discoveryServer = new ClientDiscoveryServer.ClientDiscoveryServer(discoveryPort);
                discoveryServer.Start();

                // Show main window
                MainWindow mainWindow = new MainWindow();
                MainViewModel mainViewModel = container.Resolve<MainViewModel>();
                mainWindow.DataContext = mainViewModel;
                mainWindow.ViewModel = mainViewModel;

                FlowManager.Instance.AppWindow = mainWindow;
                FlowManager.Instance.ViewModelContainer = container;

                mainWindow.Show();
            }
        }
    }
}
