using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Acabus.DataAccess
{
    public static class Csv
    {
        public static void Export<T>(IEnumerable<T> data, String filename)
        {
            File.Delete(filename);
            Boolean isCreatedHeader = false;
            foreach (var item in data)
            {
                if (!isCreatedHeader)
                {
                    File.AppendAllText(filename, CreateHeader(typeof(T)) + "\n", Encoding.UTF8);
                    isCreatedHeader = true;
                }

                File.AppendAllText(filename, GetValues(item) + "\n", Encoding.UTF8);
            }
        }

        private static string GetValues<T>(T item)
        {
            StringBuilder row = new StringBuilder();
            var type = item.GetType();
            foreach (var property in type.GetProperties())
                row.Append(property.GetValue(item).ToString()).Append(",");
            row.Remove(row.Length - 1, 1);
            return row.ToString();
        }

        private static String CreateHeader(Type type)
        {
            StringBuilder header = new StringBuilder();
            var properties = type.GetProperties();
            foreach (var property in properties)
                header.Append(property.Name).Append(",");
            header.Remove(header.Length - 1, 1);
            return header.ToString();
        }

        public static void Export(IEnumerable<IEnumerable<String>> data, IEnumerable<String> header, String filename)
        {
            File.Delete(filename);
            File.AppendAllText(filename, GetValues(header) + "\n", Encoding.UTF8);
            foreach (var dataItem in data)
                File.AppendAllText(filename, GetValues(dataItem) + "\n", Encoding.UTF8);
        }

        private static String GetValues(IEnumerable<String> data)
        {
            StringBuilder row = new StringBuilder();
            foreach (var dataItem in data)
                row.Append(dataItem).Append(",");
            row.Remove(row.Length - 1, 1);
            return row.ToString();
        }
    }
}
