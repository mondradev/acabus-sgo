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

        public static void Open()
        {
            var doc = new System.Xml.XmlDocument();
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
        {
            if (!File.Exists(path))
                CreateConfBase();

            _path = path;
        }

        private static void CreateConfBase()
        {
            throw new NotImplementedException();
        }
    }
}