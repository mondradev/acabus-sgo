using System.Linq;

namespace InnSyTech.Standard.Database
{
    /// <summary>
    /// Define la estructura básica de la sessión Sql a una base de datos relacional.
    /// </summary>
    public interface IDbSession
    {
        /// <summary>
        /// Crea una instancia persistente a partir de un tipo definido que corresponde a una tabla
        /// en la base de datos. Esto equivale a un INSERT INTO de Sql.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a persistir.</typeparam>
        /// <param name="instance">Instancia que se desea persistir.</param>
        /// <param name="referenceDepth">Especifica la profundidad de referencias a crear en caso de no existir.</param>
        /// <returns>Un true en caso que la instancia sea persistida correctamente.</returns>
        bool Create<TData>(TData instance, int referenceDepth = 0);

        /// <summary>
        /// Elimina una instancia persistida en la base de datos, si esta cuenta con campo de estado
        /// para visibilidad, solo se cambia a su valor oculto para ser ignorada en las lecturas.
        /// Esto equivale a una DELETE FROM de Sql.
        /// </summary>
        /// <typeparam name="TData">Tipo de dato de la instancia a eliminar.</typeparam>
        /// <param name="instance">Instancia a eliminar.</param>
        /// <returns>Un true si la instancia fue borrada así como sus referencias de ser necesario.</returns>
        bool Delete<TData>(TData instance);

        /// <summary>
        /// Realiza una lectura de elementos del tipo especificado que corresponde a una tabla en la
        /// base de datos, gracias a la estructura <see cref="IQueryable{T}"/> se puede aplicar
        /// filtros y ordernamiento como otros métodos que se encuentren disponibles. Esto equivale a
        /// un SELECT FROM.
        /// </summary>
        /// <typeparam name="TResult">El tipo de dato de la lectura.</typeparam>
        /// <returns>Una consulta que extrae datos de la base de datos relacional.</returns>
        IQueryable<TResult> Read<TResult>();

        /// <summary>
        /// Actualiza los atributosde la instancia persistida en la base de datos. Esto equivale a un
        /// UPDATE de Sql. Si se requiere actualizar todas las referencias se necesita establecer el
        /// parametro <paramref name="referenceDepth"/> en el número de profundidad en referencias.
        /// </summary>
        /// <typeparam name="TData">Tipo de datos de la instancia persistida.</typeparam>
        /// <param name="instance">Instancia persistida en la base de datos.</param>
        /// <param name="referenceDepth">
        /// Especifica la profundidad de referencias a actualizar en caso de no existir, se crearán.
        /// </param>
        /// <returns>
        /// Un true si la instancia fue correctamente actualizada así como sus referencias de ser necesario.
        /// </returns>
        bool Update<TData>(TData instance, int referenceDepth = 0);
    }
}