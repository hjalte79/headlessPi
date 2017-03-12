using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ConnectPi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int upCount = 0;
        static object lockObj = new object();
        const bool resolveNames = true;
        Config config = new Config();
        Dictionary<string, int> repeate = new Dictionary<string, int>();
        const string WPAFILENAME = @"wpa_supplicant.conf";

        public MainWindow()
        {
            InitializeComponent();

            IpListBox.Items.SortDescriptions.Add(
                new System.ComponentModel.SortDescription("",
                System.ComponentModel.ListSortDirection.Ascending));


            sshEnabled.IsChecked = config.sshEnabled;

            foreach (WifiListBoxItem wItem in config.wifiList)
            {
                ssidComboBox.Items.Add(wItem);
            }
            ssidComboBox.SelectedIndex = 0;

            PuttyPathTextBox.Text = config.puttyPath;
            UserNameTextBox.Text = config.userName;
            RangeTextBox.Text = config.ipRange;

            // load drive data into DriveLetterCB
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            DriveLetterCB.Items.Clear();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady && d.DriveType == DriveType.Removable)
                {
                    DriveListBoxItem itm = new DriveListBoxItem();
                    itm.Content = string.Format("[{0}] - {1} ({2})", d.Name, d.VolumeLabel, d.DriveFormat);
                    itm.info = d;
                    DriveLetterCB.Items.Add(itm);
                }

            }

            if (DriveLetterCB.Items.Count > 0)
            {
                DriveLetterCB.SelectedIndex = 0;
            }


        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            
            IpListBox.Items.Clear();
            repeate.Clear();
            string ipBase = RangeTextBox.Text;

            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();
                repeate.Add(ip, 5);

                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                p.SendAsync(ip, 100, ip);
            }
            
        }

        public void p_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            // IpListBox.Items.Add(ip);
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                if (resolveNames)
                {
                    string name;
                    try
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                        name = hostEntry.HostName;
                    }
                    catch (SocketException ex)
                    {
                        string removeErrorInVisualStudio = ex.Message;
                        name = "?";
                    }

                    CustomListBoxItem itm = new CustomListBoxItem();
                    itm.Content = string.Format("{0} ({1}) is up: ({2} ms)", ip, name, e.Reply.RoundtripTime);
                    itm.ip = ip;
                    itm.hostname = name;
                    IpListBox.Items.Add(itm);

                    // Console.WriteLine("{0} ({1}) is up: ({2} ms)", ip, name, e.Reply.RoundtripTime);
                }
                else
                {
                    IpListBox.Items.Add(string.Format("{0} is up: ({1} ms)", ip, e.Reply.RoundtripTime));
                    // Console.WriteLine("{0} is up: ({1} ms)", ip, e.Reply.RoundtripTime);
                }
                lock (lockObj)
                {
                    upCount++;
                }
            }
            else if (e.Reply == null)
            {
                IpListBox.Items.Add(string.Format("Pinging {0} failed. (Null Reply object?)", ip));
                // Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);

            }
            else
            {
                
                if (repeate.ContainsKey(ip))
                {
                    if (e.Reply.Status == IPStatus.TimedOut)
                    {
                        
                        if (repeate[ip] > 0)
                        {
                            
                            repeate[ip] = repeate[ip] - 1;

                            Ping p = new Ping();
                            p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                            p.SendAsync(ip, 100, ip);
                        }
                        else
                        {
                            repeate.Remove(ip);
                        }
                    }
                }
            }
        }

        private void selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomListBoxItem clbi = (CustomListBoxItem)IpListBox.SelectedItem;
            if (clbi != null)
            {
                IpTextBox.Text = clbi.ip;
                HostNameTextBox.Text = clbi.hostname;
            }
            
        }

        private void activateWindow(object sender, EventArgs e)
        {
            
        }

        private void puttyBTN_Click(object sender, RoutedEventArgs e)
        {
           CustomListBoxItem clbi = (CustomListBoxItem)IpListBox.SelectedItem;
            if (clbi != null)
            {
                string ip = clbi.ip;
                string username = config.userName;
                if (string.IsNullOrEmpty(username))
                {
                    Process.Start(config.puttyPath, string.Format("{0}", ip));
                }
                else
                {
                    Process.Start(config.puttyPath, string.Format("{0}@{1}", username, ip));
                }
                
            }
            
        }

        private void DriveLetterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Regex ssidRegex = new Regex("[\\s\\S]*ssid=\"([\\s\\S]*)\"");
            Regex pskRegex = new Regex("[\\s\\S]*psk=\"([\\s\\S]*)\"");

            DriveListBoxItem dlbi = (DriveListBoxItem)DriveLetterCB.SelectedItem;
            if (dlbi != null)
            {
                string drive = dlbi.info.Name;
                string path = System.IO.Path.Combine(drive, WPAFILENAME);
                if (File.Exists(path))
                {
                    string[] fileContent = System.IO.File.ReadAllLines(path);
                    WifiListBoxItem tempWifi = new WifiListBoxItem(); 
                    foreach (string line in fileContent)
                    {
                        Match ssidMatch = ssidRegex.Match(line);
                        Match pskMatch = pskRegex.Match(line);

                        if (line.IndexOf("network") != -1)
                        {
                            tempWifi = new WifiListBoxItem();
                        }

                        if (ssidMatch.Success)
                        {
                            tempWifi.ssid = ssidMatch.Groups[1].Value;
                            tempWifi.Content = ssidMatch.Groups[1].Value;
                        }

                        if (pskMatch.Success)
                        {
                            tempWifi.psk = pskMatch.Groups[1].Value;
                        }

                        if (line.IndexOf("}") != -1)
                        {
                            ssidComboBox.Items.Add(tempWifi);
                        }
                    }
                }

                if (ssidComboBox.Items.Count > 0)
                    ssidComboBox.SelectedIndex = 0;
            }

        }

        private void ssidComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WifiListBoxItem wifi = (WifiListBoxItem)ssidComboBox.SelectedItem;
            if (wifi != null)
            {

                ssidTextBox.Text = wifi.name;
                ssidTextBox.Text = wifi.ssid;
                PskTextBox.Text = wifi.psk;
            }

                
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DriveListBoxItem dlbi = (DriveListBoxItem)DriveLetterCB.SelectedItem;
            if (dlbi != null)
            {
                string drive = dlbi.info.Name;

                if (config.sshEnabled)
                {
                    string path = System.IO.Path.Combine(drive, "ssh");
                    using (StreamWriter writer = new StreamWriter(path))
                    {
                        writer.WriteLine("For headless setup, ");
                        writer.WriteLine("SSH can be enabled by placing a file named 'ssh', ");
                        writer.WriteLine("without any extension, onto the boot partition of the SD card. ");
                        writer.WriteLine("When the Pi boots, it looks for the 'ssh' file. ");
                        writer.WriteLine("If it is found, SSH is enabled, and the file is deleted. ");
                        writer.WriteLine("The content of the file does not matter: it could contain text, ");
                        writer.WriteLine("or nothing at all.");
                        writer.WriteLine("source: https://www.raspberrypi.org/documentation/remote-access/ssh/");
                        writer.WriteLine("source: https://www.raspberrypi.org/blog/a-security-update-for-raspbian-pixel/");
                    }
                }



                string wifiConfigPath = System.IO.Path.Combine(drive, "wpa_supplicant.conf");
                using (StreamWriter writer = new StreamWriter(wifiConfigPath))
                {
                    /*
                    http://raspberrypi.stackexchange.com/questions/10251/prepare-sd-card-for-wifi-on-headless-pi
                    according to the documentation its possible to put more then one 
                    wifi network in the file. But for me that didnt work. if its not possible it would make more 
                    sense to only write the selecte network to the file with the next line... 
                    (instead of the foreach loop)

                    WifiListBoxItem wifi = (WifiListBoxItem)ssidComboBox.SelectedItem;
                    */

                    foreach (WifiListBoxItem wifi in ssidComboBox.Items)
                    {
                        
                        writer.WriteLine("network={");
                        writer.WriteLine(string.Format("    ssid=\"{0}\"", wifi.ssid));
                        writer.WriteLine(string.Format("    psk=\"{0}\"", wifi.psk));
                        writer.WriteLine("}");
                        writer.WriteLine("");
                    }
                }

            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            WifiListBoxItem itm = new WifiListBoxItem();
            itm.name = ssidComboBox.Text;
            itm.ssid = ssidTextBox.Text;
            itm.psk = PskTextBox.Text;
            ssidComboBox.Items.Add(itm);

            this.saveWifiList();
        }

        private void sshEnabled_Click(object sender, RoutedEventArgs e)
        {
            config.sshEnabled = (bool)sshEnabled.IsChecked;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            WifiListBoxItem wifi = (WifiListBoxItem)ssidComboBox.SelectedItem;
            if (wifi != null)
            {
                ssidComboBox.Items.Remove(wifi);
                ssidComboBox.SelectedIndex = 0;
            }

            this.saveWifiList();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            ssidComboBox.Text = "";
            ssidTextBox.Text = "";
            PskTextBox.Text = "";
        }

        public void saveWifiList()
        {
            List<WifiListBoxItem> tempList = new List<WifiListBoxItem>();
            foreach (var wifiItm in ssidComboBox.Items)
                tempList.Add((WifiListBoxItem)wifiItm);

            config.wifiList = tempList;
        }

        private void PuttyPathChanged(object sender, TextChangedEventArgs e)
        {
            if (File.Exists(PuttyPathTextBox.Text))
            {
                puttyBTN.IsEnabled = true;
                PuttyPathTextBox.Foreground = Brushes.Black;
            }
            else
            {
                puttyBTN.IsEnabled = false;
                PuttyPathTextBox.Foreground = Brushes.Red;
            }
            config.puttyPath = PuttyPathTextBox.Text;
            
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "txt files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                config.puttyPath = openFileDialog1.FileName;
                PuttyPathTextBox.Text = config.puttyPath;
            }
        }

        private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            config.userName = UserNameTextBox.Text;
        }

        private void RangeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex(@"[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}\.");
            Match match = regex.Match(RangeTextBox.Text);
            RangeTextBox.Foreground = (match.Success) ? Brushes.Black : Brushes.Red;

            config.ipRange = RangeTextBox.Text;
        }
    }
}
