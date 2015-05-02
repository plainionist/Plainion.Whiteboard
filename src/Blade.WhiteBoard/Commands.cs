using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Plainion.WhiteBoard
{
    class Commands
    {
        public static RoutedCommand DiscardAndClose = new RoutedCommand();
        public static RoutedCommand SaveAndClose = new RoutedCommand();
    }
}
