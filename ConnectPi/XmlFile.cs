using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace ConnectPi
{
    public class XmlFile
    {
        protected XmlDocument xmlDoc = new XmlDocument();
        protected XmlNode rootNode = null;
        protected string thisFilePath;

        public XmlFile() { }
        public XmlFile(string pathToFile)
        {
            this.loadFile(pathToFile);
        }

        public void loadFile(string pathToFile)
        {
            thisFilePath = pathToFile;
            xmlDoc.Load(pathToFile);
        }

        public string getValueByXpath(string xPath)
        {
            XmlNodeList nodeList = this.getNodeList(xPath);
            if (nodeList.Count == 1)
            {
                return nodeList[0].InnerText;
            }
            else if (nodeList.Count > 1)
            {
                throw new Exception("More then 1 node found in xPath: " + xPath);
            }

            return "";
        }

        public XmlNodeList getNodeList(string xPath)
        {
             return xmlDoc.DocumentElement.SelectNodes(xPath);
        }

        public void save()
        {
            xmlDoc.Save(thisFilePath);
        }
    }
}
