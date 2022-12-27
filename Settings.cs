using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Weland
{
    // from http://www.codeproject.com/KB/XML/quickndirtyxml.aspx
    public class Settings
    {
        XmlDocument xmlDocument = new XmlDocument();
        string applicationData;
        string documentPath;

        private CultureInfo ic = CultureInfo.InvariantCulture;

        public Settings()
        {
            applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//Weland";
            documentPath = applicationData + "//settings.xml";

            try
            {
                xmlDocument.Load(documentPath);
            }
            catch { xmlDocument.LoadXml("<settings></settings>"); }
        }

        public int GetSetting(string xPath, int defaultValue)
        {
            return Convert.ToInt32(GetSetting(xPath, Convert.ToString(defaultValue, ic)), ic);
        }

        public void PutSetting(string xPath, int value)
        {
            PutSetting(xPath, Convert.ToString(value, ic));
        }

        public bool GetSetting(string xPath, bool defaultValue)
        {
            return Convert.ToBoolean(GetSetting(xPath, Convert.ToString(defaultValue, ic)), ic);
        }

        public double GetSetting(string xPath, double defaultValue)
        {
            return Convert.ToDouble(GetSetting(xPath, Convert.ToString(defaultValue, ic)), ic);
        }

        public void PutSetting(string xPath, double value)
        {
            PutSetting(xPath, Convert.ToString(value, ic));
        }

        public void PutSetting(string xPath, bool value)
        {
            PutSetting(xPath, Convert.ToString(value, ic));
        }

        public string GetSetting(string xPath, string defaultValue)
        {
            var xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode != null)
            {
                return xmlNode.InnerText;
            }
            else
            {
                return defaultValue;
            }
        }

        public void PutSetting(string xPath, string value)
        {
            var xmlNode = xmlDocument.SelectSingleNode("settings/" + xPath);
            if (xmlNode == null)
            {
                xmlNode = createMissingNode("settings/" + xPath);
            }
            xmlNode.InnerText = value;
            try
            {
                if (!Directory.Exists(applicationData))
                {
                    Directory.CreateDirectory(applicationData);
                }
                xmlDocument.Save(documentPath);
            }
            catch (Exception)
            {
            }
        }

        private XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split(new char[] { '/' });
            string currentXPath = "";
            XmlNode? testNode = null;
            XmlNode currentNode = xmlDocument.SelectSingleNode("settings")!;
            foreach (string xPathSection in xPathSections)
            {
                currentXPath += xPathSection;
                testNode = xmlDocument.SelectSingleNode(currentXPath);
                if (testNode == null)
                {
                    currentNode.InnerXml += "<" +
                    xPathSection + "></" +
                    xPathSection + ">";
                }
                currentNode = xmlDocument.SelectSingleNode(currentXPath)!;
                currentXPath += "/";
            }
            return currentNode;
        }
    }
}
