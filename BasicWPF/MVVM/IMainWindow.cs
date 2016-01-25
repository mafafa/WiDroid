using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BasicWPF.MVVM
{
    public interface IMainWindow
    {
        MainViewModel ViewModel { get; set; }
    }
}
