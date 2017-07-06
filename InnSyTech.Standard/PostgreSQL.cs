using Npgsql;
using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace InnSyTech.Standard
{
    public class PostgreSQL
    {
        private const String _CONNECTION = "Server={0};Port={1};User Id={2};Password={3};Database={4};";

        public String Username { get; protected set; }
        public String Passoword { get; protected set; }
        public String Host { get; protected set; }
        public String DataBase { get; protected set; }
        public UInt16 Port { get; protected set; }

        public static PostgreSQL CreateConnection(String host, UInt16 port, String username, String password, String database)
        {
            return new PostgreSQL()
            {
                Host = host,
                Port = port,
                Username = username,
                Passoword = password,
                DataBase = database
            };
        }

        protected PostgreSQL() { }

        public virtual String[][] ExecuteQuery(String statement)
        {
            try
            {
                String response = null;
                if (String.IsNullOrEmpty(statement)) return null;
                using (NpgsqlConnection connection = InitializeConnection())
                {
                    if (connection != null) connection.Open();
                    else return null;

                    if (connection.State == ConnectionState.Open)
                    {
                        NpgsqlCommand command = new NpgsqlCommand(statement, connection);
                        NpgsqlDataReader reader = command.ExecuteReader();
                        StringBuilder header = new StringBuilder();
                        Boolean readHeader = false;
                        Int64 rowsCount = 0;
                        StringBuilder rows = new StringBuilder();
                        while (reader.Read())
                        {
                            if (rowsCount > 0) rows.Append('|');
                            for (Int16 i = 0; i < reader.FieldCount; i++)
                            {
                                if (!readHeader)
                                {
                                    header.Append(reader.GetName(i));
                                    if (i + 1 < reader.FieldCount)
                                        header.Append(",");
                                }
                                var value = reader[i];
                                if (value is DateTime && (value as DateTime?).Value.Year == 1)
                                    value = (value as DateTime?).Value.TimeOfDay;
                                rows.Append(value.ToString());
                                if (i + 1 < reader.FieldCount) rows.Append(",");
                            }
                            rowsCount++;
                            readHeader = true;
                        }
                        response = header.Append("|").Append(rows.ToString()).ToString();
                    }
                    connection.Close();
                }
                return String.IsNullOrEmpty(response) ? null : ProcessResponse(response);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ERROR");
            }
            return new String[][] { };
        }

        private NpgsqlConnection InitializeConnection()
        {
            string connection = String.Format(_CONNECTION,
                        Host, Port, Username, Passoword, DataBase);
            return new NpgsqlConnection(connection);
        }

        protected String[][] ProcessResponse(string response)
        {
            int columnsCount = 0;
            String[] rows = response.Split('|');
            int rowsCount = rows.Length;
            if (rowsCount > 0)
            {
                columnsCount = rows[0].Split(',').Length;
            }
            if (columnsCount > 0 && rowsCount > 0)
            {
                String[][] tempData = new String[rowsCount][];
                for (int i = 0; i < rowsCount; i++)
                {
                    if (Regex.IsMatch(rows[i], "[0-9]{1,}\\srow"))
                    {
                        rowsCount--;
                        break;
                    }
                    tempData[i] = rows[i].Split(',');
                }
                String[][] data = new String[rowsCount][];
                for (int i = 0; i < rowsCount; i++)
                {
                    data[i] = tempData[i];
                }
                return data;
            }
            return null;
        }
    }
}
