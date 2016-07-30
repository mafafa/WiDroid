using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BasicWPF.MVVM
{
    public interface IMainViewModel : IViewModel
    {
        IViewModel CurrentViewModel { get; set; }
    }
}
