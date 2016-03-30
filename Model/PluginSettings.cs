using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using Advanced_Combat_Tracker;

namespace Statter
{
    public class PluginSettings
    {
        private string _settingsFile = string.Empty;
        public static string SettingsFile
        {
            get
            {
                return _instance._settingsFile;
            }
        }

        private bool _parseOnImport = true;
        public static bool ParseOnImport
        {
            get
            {
                return _instance._parseOnImport;
            }
            set
            {
                _instance._parseOnImport = value;
                _instance.Save();
            }
        }

        private bool _stepLines = true;
        public static bool StepLines
        {
            get
            {
                return _instance._stepLines;
            }
            set
            {
                _instance._stepLines = value;
                _instance.Save();
            }
        }

        private List<Stat> _selectedStats = new List<Stat>();
        public static List<Stat> SelectedStats
        {
            get
            {
                return _instance._selectedStats;
            }
            set
            {
                _instance._selectedStats = value;
                _instance.Save();
            }
        }

        private static PluginSettings _instance = new PluginSettings();
        private PluginSettings()
        {
            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, @"Config\Statter.config.xml");
            Load();
        }

        private void Load()
        {
            if (!File.Exists(_settingsFile)) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(_settingsFile);
            XmlNode rootNode = doc.SelectSingleNode("Settings");

            _parseOnImport = RetrieveSetting<bool>(rootNode, "ParseOnImport");
            _stepLines = RetrieveSetting<bool>(rootNode, "StepLines");

            LoadStats(rootNode.SelectSingleNode("Stats"));
        }

        private void Save()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));

            XmlElement rootNode = doc.CreateElement("Settings");
            doc.AppendChild(rootNode);

            AttachChildNode(rootNode, "ParseOnImport", _parseOnImport.ToString());
            AttachChildNode(rootNode, "StepLines", _stepLines.ToString());

            XmlElement statsNode = AttachChildNode(rootNode, "Stats", null);
            SaveStats(statsNode);

            doc.Save(SettingsFile);
        }

        private T RetrieveSetting<T>(XmlNode attachPoint, string name)
        {
            T settingVal = default(T);

            XmlNode settingNode = attachPoint.SelectSingleNode(name);
            if (settingNode != null)
                settingVal = (T)Convert.ChangeType(settingNode.InnerText, typeof(T));

            return settingVal;
        }

        private void LoadStats(XmlNode rootNode)
        {
            if (rootNode == null) return;

            _selectedStats.Clear();
            foreach (XmlNode statNode in rootNode.SelectNodes("Stat"))
            {
                string name = "";
                Color colour = Stat.DEFAULT_COLOUR;

                foreach (XmlNode childNode in statNode.ChildNodes)
                {
                    string nodeVal = childNode.InnerText.Trim();
                    switch (childNode.Name.ToLower())
                    {
                        case "name":
                            name = nodeVal;
                            break;
                        case "colour":
                            TryParseColour(nodeVal, ref colour);
                            break;
                    }
                }

                try
                {
                    Stat stat = new Stat(name)
                    {
                        Colour = colour
                    };
                    if (!_selectedStats.Contains(stat))
                        _selectedStats.Add(stat);
                }
                catch { }
            }
        }

        private void SaveStats(XmlElement attachPoint)
        {
            foreach (Stat stat in _selectedStats)
            {
                XmlElement statNode = AttachChildNode(attachPoint, "Stat", null);
                AttachChildNode(statNode, "Name", stat.Name);
                AttachChildNode(statNode, "Colour", ColourToString(stat.Colour));
            }
        }

        private XmlElement AttachChildNode(XmlElement parentNode, string name, string value)
        {
            XmlElement childNode = parentNode.OwnerDocument.CreateElement(name);
            if (value != null)
                childNode.InnerText = value;
            parentNode.AppendChild(childNode);

            return childNode;
        }

        private bool TryParseColour(string colourString, ref Color colour)
        {
            bool success = false;

            try
            {
                int r, g, b;
                string[] rgbParts = colourString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (int.TryParse(rgbParts[0], out r) && int.TryParse(rgbParts[1], out g) && int.TryParse(rgbParts[2], out b))
                    colour = Color.FromArgb(Math.Min(255, Math.Max(0, r)), Math.Min(255, Math.Max(0, g)), Math.Min(255, Math.Max(0, b)));
                success = true;
            }
            catch { }

            return success;
        }

        private string ColourToString(Color colour)
        {
            return string.Format("{0},{1},{2}", colour.R, colour.G, colour.B);
        }
    }
}
