using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace InnSyTech.Standard.Database.Linq
{
    /// <summary>
    /// Añade métodos que permiten extender la funcionalidad de Linq para la extracción de tipos
    /// complejos de la base de datos.
    /// </summary>
    public static class DbQueryable
    {

        /// <summary>
        /// Carga todas las referencias según el nivel de profundidad que se desea avanzar. Si el
        /// tipo A tiene referencias del tipo B y este a su vez del tipo C, para cargar estas
        /// referencias se deberá indicar un nivel 2.
        /// </summary>
        /// <typeparam name="TSource">Tipo de la instanica a cargar todas sus referencias.</typeparam>
        /// <param name="source">
        /// Consulta origen a la cual se especificará que cargue sus respectivas referencias.
        /// </param>
        /// <param name="depth">
        /// Profundidad de la carga de referencia. Use 0 para no cargar referencias, 1 para el primer nivel.
        /// </param>
        /// <returns>Una consulta con carga de referencias.</returns>
        public static IQueryable<TSource> LoadReference<TSource>(this IQueryable<TSource> source, int depth)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "La colección origen no puede ser nula.");

            if (depth < 0)
                throw new ArgumentOutOfRangeException(nameof(depth), 
                    "El nivel de profundidad de la carga de referencia no puede ser un número negativo.");

            MethodInfo loadReferenceMethod = typeof(DbQueryable)
                .GetMethods().Single(m => m.Name.Equals(nameof(LoadReference)) && m.IsGenericMethod)
                .MakeGenericMethod(typeof(TSource));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(loadReferenceMethod, source.Expression, Expression.Constant(depth))
            );
        }
        
    }
}