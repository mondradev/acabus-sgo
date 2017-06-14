using Acabus.Utils.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Acabus.Modules.Configurations
{
    public interface IConfigurable
    {

        List<Tuple<String, Func<Object>>> PreviewData { get; }

        List<Tuple<String, ICommand>> Commands { get; }

        String Title { get; }
    }
}
