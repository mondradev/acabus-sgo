using System;
using System.IO;

namespace InnSyTech.Standard.Configuration
{
    public static class ConfigurationManager
    {
        private static string _path;

        private static SettingCollection _settings;

        public static String ConfigurationFile => _path;

        public static SettingCollection Settings => _settings;

        public static void Load(String path)
        {
            SetConfigurationFile(path);
            Open();
        }

        public static void Open()
        {
            var doc = new System.Xml.XmlDocument();

            if (!File.Exists(_path))
                doc = CreateConfBase();
            else
                doc.Load(_path);

            _settings = new SettingCollection(doc);
        }

        public static void Reload()
            => Open();

        public static void Save()
        {
            var xmlDoc = _settings.GetXmlDocument();
            xmlDoc.Save(_path);
        }

        public static void SetConfigurationFile(String path)
            => _path = path;

        private static System.Xml.XmlDocument CreateConfBase()
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf - 8""?>
                            <configuration>
                            </configuration>
            ");
            return doc;
        }
    }
}