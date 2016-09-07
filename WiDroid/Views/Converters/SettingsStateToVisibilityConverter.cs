using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

using Helper.CommonConverters;
using WiDroid.ViewModels;


namespace WiDroid.Views.Converters
{
    public class SettingsStateToVisibilityConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SettingsViewModel.SettingsState settingState = (SettingsViewModel.SettingsState)value;

            switch (settingState)
            {
                case SettingsViewModel.SettingsState.Default:
                    return Visibility.Collapsed;
                case SettingsViewModel.SettingsState.Changing:
                    return Visibility.Visible;
                case SettingsViewModel.SettingsState.Changed:
                    return Visibility.Collapsed;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
