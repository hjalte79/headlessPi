using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConnectPi
{
    class WifiListBoxItem : ListBoxItem
    {
        public string name
        {
            get { return (string)this.Content; }
            set { this.Content = value; }
        }
        public string ssid { get; set; }
        public string psk { get; set; }
    }
}
