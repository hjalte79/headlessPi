using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
namespace ConnectPi
{
    class DriveListBoxItem : ListBoxItem
    {
        public DriveInfo info { get; set; }
    }
}
