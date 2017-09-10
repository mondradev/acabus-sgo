using Opera.Acabus.Core.Modules.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opera.Acabus.TrunkMonitor.Config
{
    public class TrunkMonitorConfig : IConfigurable
    {
        public List<Tuple<string, ICommand>> Commands => throw new NotImplementedException();

        public List<Tuple<string, Func<object>>> PreviewData => throw new NotImplementedException();

        public string Title => throw new NotImplementedException();
    }
}
