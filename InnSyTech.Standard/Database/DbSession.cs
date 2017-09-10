﻿using InnSyTech.Standard.Database.Linq;
using InnSyTech.Standard.Database.Utils;
using InnSyTech.Standard.Utils;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Implementa <see cref="IDbSession"/> para proporcionar el acceso a la base de datos a través
    /// de las funciones de crear, leer, actualizar y eliminar.
    /// </summary>
    internal sealed class DbSession : IDbSession
    {
        /// <summary>
        /// Crea una nueva instancia de <see cref="DbSession"/>.
        /// </summary>
        /// <param name="dbConnection">La conexión a la base de datos.</param>
        /// <param name="dialect">El dialecto de comunicación de la base de datos.</param>
        public DbSession(DbConnection dbConnection, IDbDialect dialect)
            => Provider = new DbProvider(dbConnection, dialect);

        /// <summary>
        /// Obtiene el proveedor de consultas para la base de datos.
        /// </summary>
        public DbProvider Provider { get; }

        /// <summary>
        /// Crea una instancia persistente a partir de un tipo definido que corresponde a una tabla
        /// en la base de datos. Esto equivale a un INSERT INTO de Sql.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a persistir.</typeparam>
        /// <param name="instance">Instancia que se desea persistir.</param>
        /// <param name="referenceDepth">Especifica la profundidad de referencias a crear en caso de no existir.</param>
        /// <returns>Un true en caso que la instancia sea persistida correctamente.</returns>
        public bool Create<TData>(TData instance, int referenceDepth = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Elimina una instancia persistida en la base de datos, si esta cuenta con campo de estado
        /// para visibilidad, solo se cambia a su valor oculto para ser ignorada en las lecturas.
        /// Esto equivale a una DELETE FROM de Sql.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a eliminar.</typeparam>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un true si la instancia fue borrada así como sus referencias de ser necesario.</returns>
        public bool Delete<TData>(TData instance)
        {
            DbTransaction transaction = Provider.BeginTransaction();
            DbCommand command = Provider.CreateCommand(transaction);

            try
            {
                var tablename = DbHelper.GetEntityName(instance.GetType());
                var primaryKey = DbHelper.GetPrimaryKey(instance.GetType());

                var parameter = command.CreateParameter();

                command.CommandText = String.Format("DELETE FROM {0} WHERE {1} = @key", tablename, primaryKey.Name);

                parameter.ParameterName = "@key";
                parameter.Value = primaryKey.GetValue(instance);

                command.Parameters.Add(parameter);

                Trace.WriteLine($"Ejecutando: {command.CommandText}", "DEBUG");

                command.ExecuteNonQuery();

                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                Trace.WriteLine( ex.Message.JoinLines(), "ERROR");

                return false;
            }
            finally
            {
                if (transaction != null)
                    Provider.EndTransaction(transaction);

                if (command != null)
                    command.Dispose();

                Provider.CloseConnection();
            }
        }

        /// <summary>
        /// Realiza una lectura de elementos del tipo especificado que corresponde a una tabla en la
        /// base de datos, gracias a la estructura <see cref="IQueryable{T}"/> se puede aplicar
        /// filtros y ordernamiento como otros métodos que se encuentren disponibles. Esto equivale a
        /// un SELECT FROM.
        /// </summary>
        /// <typeparam name="TResult">El tipo de dato de la lectura.</typeparam>
        /// <returns>Una consulta que extrae datos de la base de datos relacional.</returns>
        public IQueryable<TResult> Read<TResult>()
            => new DbSqlQuery<TResult>(Provider);

        /// <summary>
        /// Actualiza los atributosde la instancia persistida en la base de datos. Esto equivale a un
        /// UPDATE de Sql. Si se requiere actualizar todas las referencias se necesita colocar el
        /// parametro <paramref name="cascade"/> en true.
        /// </summary>
        /// <typeparam name="TData">Tipo de datos de la instancia persistida.</typeparam>
        /// <param name="instance">Instancia persistida en la base de datos.</param>
        /// <param name="referenceDepth">
        /// Especifica la profundidad de referencias a actualizar en caso de no existir, se crearán.
        /// </param>
        /// <returns>
        /// Un true si la instancia fue correctamente actualizada así como sus referencias de ser necesario.
        /// </returns>
        public bool Update<TData>(TData instance, int referenceDepth = 0)
        {
            throw new NotImplementedException();
        }


    }
}