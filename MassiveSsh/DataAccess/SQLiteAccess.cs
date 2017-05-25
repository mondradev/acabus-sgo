﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Acabus.DataAccess
{
    public static class SQLiteAccess
    {
        private static SQLiteConnection _connection;
        private static SQLiteCommand _command;

        private static Object _lock = new object();
        private static SQLiteTransaction _transaction;
        private static bool _inTransaction;

        static SQLiteAccess()
        {
            _inTransaction = false;
            _connection = new SQLiteConnection("Data Source=Resources/acabus_data.dat;Version=3;Password=acabus*data*dat");
            _command = _connection.CreateCommand();
        }



        public static Object[][] ExecuteQuery(String query, out String[] header)
        {
            List<Object[]> responseData = new List<Object[]>();
            lock (_lock)
            {
                if (_connection.State != System.Data.ConnectionState.Open) _connection.Open();
                if (_inTransaction)
                    _command.Transaction = _transaction;
                _command.CommandText = query;
                _command.CommandType = System.Data.CommandType.Text;

                var response = _command.ExecuteReader();
                int i = 0;
                header = new string[response.FieldCount];
                for (int j = 0; j < response.FieldCount; j++)
                    header[j] = response.GetName(j);
                while (response.Read())
                {
                    responseData.Add(new Object[response.FieldCount]);
                    for (int j = 0; j < responseData[i].Length; j++)
                        try
                        {
                            responseData[i][j] = response[j];
                        }
                        catch (Exception)
                        {
                            responseData[i][j] = response.GetString(j);
                        }
                    i++;
                }
                response.Close();

                if (!_inTransaction)
                    _connection.Close();
            }
            return responseData.ToArray();
        }

        public static Object[][] ExecuteQuery(String query)
            => ExecuteQuery(query, out String[] header);

        public static Int16 Execute(String query)
        {
            Int16 rows = 0;
            lock (_lock)
            {
                if (_connection.State != System.Data.ConnectionState.Open) _connection.Open();
                if (_inTransaction)
                    _command.Transaction = _transaction;
                _command.CommandText = query;

                rows = (Int16)_command.ExecuteNonQuery();
                if (!_inTransaction)
                    _connection.Close();
            }
            return rows;
        }

        public static void BeginTransaction()
        {
            if (_connection.State != System.Data.ConnectionState.Open) _connection.Open();

            _transaction = _connection.BeginTransaction();
            _inTransaction = true;
        }

        public static void RollBack()
        {
            _transaction.Rollback();
            _inTransaction = false;
            _transaction.Dispose();
            _connection.Close();
        }

        public static void Commit()
        {
            _transaction.Commit();
            _inTransaction = false;
            _transaction.Dispose();
            _connection.Close();
        }

        public static String ToSqliteFormat(this DateTime datetime)
        {
            return String.Format("{0:yyyy-MM-dd HH:mm:ss}", datetime);
        }

        public static object Select(string query)
        {
            object[][] response = ExecuteQuery(query);
            return response?[0]?[0];
        }
    }
}
