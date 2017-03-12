using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConnectPi
{
    class CustomListBoxItem : ListBoxItem
    {
        public string ip { get; set; }
        public string hostname { get; set; }
    }
}
