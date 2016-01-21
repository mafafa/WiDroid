using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiDroid
{
    public interface IView<TViewModel> where TViewModel : IViewModel
    {
        TViewModel ViewModel { get; set; }
    }
}
