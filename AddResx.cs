using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        string razorFile = @"c:\Users\fabio.dagostino\Desktop\RazorEnhanced-release-1.0\Razor\UI\Razor.cs";
        string resxEn = @"c:\Users\fabio.dagostino\Desktop\RazorEnhanced-release-1.0\Razor\RazorEnhanced\UI\Strings.resx";
        string resxIt = @"c:\Users\fabio.dagostino\Desktop\RazorEnhanced-release-1.0\Razor\RazorEnhanced\UI\Strings.it.resx";

        UpdateResx(razorFile, "MainForm", resxEn, resxIt);
    }

    static void UpdateResx(string sourceFile, string prefix, string resxEn, string resxIt)
    {
        string content = File.ReadAllText(sourceFile);
        var matches = Regex.Matches(content, @"^\s*this\.([a-zA-Z0-9_]+)\.Text\s*=\s*""([^""]*)"";", RegexOptions.Multiline);

        var toAdd = new Dictionary<string, string>();
        foreach (Match m in matches)
        {
            string control = m.Groups[1].Value;
            string text = m.Groups[2].Value;
            string key = prefix + "." + control + ".Text";
            if (!toAdd.ContainsKey(key))
                toAdd[key] = text;
        }

        AddMissingToResx(resxEn, toAdd);
        AddMissingToResx(resxIt, toAdd);
        Console.WriteLine($"Found {toAdd.Count} unique control texts in {sourceFile}.");
    }

    static void AddMissingToResx(string resxPath, Dictionary<string, string> entries)
    {
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.Load(resxPath);
        XmlNode root = doc.DocumentElement;

        HashSet<string> existingKeys = new HashSet<string>();
        foreach (XmlNode child in root.ChildNodes)
        {
            if (child.Name == "data" && child.Attributes["name"] != null)
            {
                existingKeys.Add(child.Attributes["name"].Value);
            }
        }

        int added = 0;
        foreach (var kvp in entries)
        {
            if (!existingKeys.Contains(kvp.Key))
            {
                XmlElement data = doc.CreateElement("data");
                data.SetAttribute("name", kvp.Key);
                data.SetAttribute("xml:space", "preserve");
                
                XmlElement value = doc.CreateElement("value");
                value.InnerText = kvp.Value;
                
                data.AppendChild(value);
                root.AppendChild(data);
                added++;
            }
        }

        if (added > 0)
        {
            doc.Save(resxPath);
            Console.WriteLine($"Added {added} missing keys to {resxPath}");
        }
    }
}
