using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConnectPi
{
    class Config:XmlFile
    {
        const string CONFIGFILENAME = "config.xml";

        public Config():base(CONFIGFILENAME)
        {
            string appPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string configFilePath = System.IO.Path.GetDirectoryName(appPath);
            configFilePath = System.IO.Path.Combine(configFilePath, CONFIGFILENAME);
            this.loadFile(configFilePath);
        }

        public string puttyPath
        {
            get
            {
                return this.getValueByXpath("/config/paths/putty");
            }
            set
            {
                XmlNode node = xmlDoc.SelectSingleNode("/config/paths/putty");
                node.InnerText = value;
                this.save();
            }
        }

        public string userName
        {
            get
            {
                XmlNode node = xmlDoc.SelectSingleNode("/config/userProfiles/user/name");
                return node.InnerText;
            }
            set
            {
                XmlNode node = xmlDoc.SelectSingleNode("/config/userProfiles/user/name");
                node.InnerText = value;
                this.save();
            }
        }

        public string ipRange
        {
            get
            {
                XmlNode node = xmlDoc.SelectSingleNode("/config/ipRange");
                return node.InnerText;
            }
            set
            {
                XmlNode node = xmlDoc.SelectSingleNode("/config/ipRange");
                node.InnerText = value;
                this.save();
            }
        }

        public bool sshEnabled
        {
            get
            {
                string waarde = this.getValueByXpath("/config/sdCard/sshEnabled");
                if (waarde.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                XmlNode node = xmlDoc.SelectSingleNode("/config/sdCard/sshEnabled");
                node.InnerText = value.ToString();
                this.save();
            }
        }

        public List<WifiListBoxItem> wifiList
        {
            get
            {
                XmlNodeList wList = this.getNodeList("/config/sdCard/wifiNetworks/*");
                List<WifiListBoxItem> tempList = new List<WifiListBoxItem>();

                foreach (XmlNode network in wList)
                {
                    WifiListBoxItem tempItm = new WifiListBoxItem();
                    // XmlNode test = network.SelectSingleNode("//name");
                    tempItm.name = network.SelectSingleNode("name").InnerText;
                    tempItm.ssid = network.SelectSingleNode("ssid").InnerText;
                    tempItm.psk = network.SelectSingleNode("psk").InnerText;
                    tempList.Add(tempItm);
                }
                return tempList;
            }
            set
            {
                XmlNode node = xmlDoc.SelectSingleNode("/config/sdCard/wifiNetworks");
                node.RemoveAll();

                foreach (WifiListBoxItem wifiItm in value)
                {

                    XmlElement network = xmlDoc.CreateElement("network");

                    XmlElement name = xmlDoc.CreateElement("name");
                    name.InnerText = wifiItm.name;
                    network.AppendChild(name);

                    XmlElement ssid = xmlDoc.CreateElement("ssid");
                    ssid.InnerText = wifiItm.ssid;
                    network.AppendChild(ssid);

                    XmlElement psk = xmlDoc.CreateElement("psk");
                    psk.InnerText = wifiItm.psk;
                    network.AppendChild(psk);

                    node.AppendChild(network);
                }
                this.save();
            }
        }
    }
}
