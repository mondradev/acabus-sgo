﻿using System;
using System.Diagnostics;

namespace InnSyTech.Standard.SecureShell
{
    public sealed class SshPsql : PostgreSQL
    {
        private const String _CONNECTION_BY_SSH = "PGPASSWORD='{0}' {1}/bin/psql -U {2} -d {3} -p {4} -F ',' -R '|' --no-align -c \"{5}\" | grep -E '[\\||,|0-9A-Za-z]'";

        private SshPsql()
        {
        }

        public Int16 Attempts { get; set; }
        public String PasswordSsh { get; private set; }
        public String PgPath { get; set; }
        public String UsernameSsh { get; private set; }

        public static SshPsql CreateConnection(String pgpath, String host, UInt16 port, String usernamedb, String passworddb,
                                                    String database, String usernamessh, String passwordssh)
        {
            return new SshPsql()
            {
                PgPath = pgpath,
                Username = usernamedb,
                Passoword = passworddb,
                DataBase = database,
                Port = port,
                Host = host,
                UsernameSsh = usernamessh,
                PasswordSsh = passwordssh,
                Attempts = 5
            };
        }

        public override string[][] ExecuteQuery(string statement)
        {
            try
            {
                String[][] response = base.ExecuteQuery(statement);
                if (response.Length < 1)
                    throw new Exception(String.Format("Host {0} falló al realizar consulta PSQL a través del controlador de PostgreSQL", Host));
                return response;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "DEBUG");
                Trace.WriteLine(String.Format("Intentando en {0} por SSH con la credenciales\nUsername: {1}\nPassword: ******", Host, UsernameSsh), "DEBUG");
                return ExecuteQuerySsh(statement);
            }
        }

        public String[][] ExecuteQuerySsh(String query)
        {
            query = query.Replace("\"", "\\\"").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
            Int16 attempts = 0;
            String response = "";
            if (String.IsNullOrEmpty(query)) return null;
            while (attempts < Attempts)
            {
                try
                {
                    using (Ssh ssh = new Ssh(this.Host, this.UsernameSsh, this.PasswordSsh))
                    {
                        if (ssh.IsConnected())
                        {
                            response = ssh.SendCommand(String.Format(_CONNECTION_BY_SSH,
                                                                        this.Passoword,
                                                                        this.PgPath,
                                                                        this.Username,
                                                                        this.DataBase,
                                                                        this.Port,
                                                                        query));
                            if (String.IsNullOrEmpty(response))
                                throw new Exception(String.Format("El host {0} no respondió", this.Host));
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    attempts++;
                    Trace.WriteLine(ex.Message, "DEBUG");
                    Trace.WriteLine(String.Format("El host {0} intentará nuevamente realizar la consulta: intento {1}/{2}", this.Host, attempts, Attempts), "INFO");
                }
            }
            if (response.Contains("ERROR:"))
                throw new Exception(response.Substring(response.IndexOf("ERROR: ") + 7));
            return String.IsNullOrEmpty(response) ? null : ProcessResponse(response);
        }
    }
}