using System;
using System.IO;
using System.Xml;

class Program
{
    static void Main(string[] args)
    {
        string newBase64 = File.ReadAllText(@"..\new_icon_base64.txt").Trim();
        string[] resxFiles = Directory.GetFiles(@"..\Razor", "*.resx", SearchOption.AllDirectories);

        foreach (string file in resxFiles)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                bool modified = false;

                XmlNodeList dataNodes = doc.SelectNodes("//data");
                foreach (XmlNode n in dataNodes)
                {
                    XmlAttribute nameAttr = n.Attributes["name"];
                    XmlAttribute typeAttr = n.Attributes["type"];

                    if (nameAttr != null && (nameAttr.Value == "$this.Icon" || nameAttr.Value == "Icon1"))
                    {
                        if (typeAttr != null && typeAttr.Value.Contains("System.Drawing.Icon"))
                        {
                            XmlNode valNode = n.SelectSingleNode("value");
                            if (valNode != null && valNode.InnerText != newBase64)
                            {
                                valNode.InnerText = newBase64;
                                modified = true;
                            }
                        }
                    }
                }

                if (modified)
                {
                    doc.Save(file);
                    Console.WriteLine("Updated " + Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed on " + file + ": " + ex.Message);
            }
        }
    }
}
