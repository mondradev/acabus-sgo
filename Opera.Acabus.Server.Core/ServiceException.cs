using InnSyTech.Standard.Net.Communications.AdaptiveMessages;
using System;

namespace Opera.Acabus.Server.Core
{
    /// <summary>
    /// Define el tipo de excepción ocurrida dentro de los servicios.
    /// </summary>
    public class ServiceException : Exception
    {
        /// <summary>
        /// Obtiene el código de error de la petición.
        /// </summary>
        public AdaptiveMessageResponseCode Code { get; }

        /// <summary>
        /// Obtiene el nombre de la función donde ocurrió la excepción.
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Obtiene el nombre del módulo donde ocurrió la excepción.
        /// </summary>
        public string ModuleName { get; }

        /// <summary>
        /// Crea una instancia nueva de error con un código de respuesta <see cref="AdaptiveMessageResponseCode.INTERNAL_SERVER_ERROR"/>.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        /// <param name="functionName">Nombre de la función donde surge la excepción.</param>
        /// <param name="moduleName">Nombre del módulo donde surge la excepción.</param>
        public ServiceException(string message, string functionName, string moduleName) :
            this(message, AdaptiveMessageResponseCode.INTERNAL_SERVER_ERROR, functionName, moduleName)
        { }

        /// <summary>
        /// Crea una instancia nueva de error.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        /// <param name="code">Código de respuesta.</param>
        /// <param name="functionName">Nombre de la función donde surge la excepción.</param>
        /// <param name="moduleName">Nombre del módulo donde surge la excepción.</param>
        public ServiceException(string message, AdaptiveMessageResponseCode code, string functionName, string moduleName) :
            this(message, code, functionName, moduleName, null)
        { }

        /// <summary>
        /// Crea una instancia nueva de error especificando una excepción interna.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        /// <param name="code">Código de respuesta.</param>
        /// <param name="functionName">Nombre de la función donde surge la excepción.</param>
        /// <param name="moduleName">Nombre del módulo donde surge la excepción.</param>
        /// <param name="innerException">Excepción interna causante el error actual.</param>
        public ServiceException(string message, AdaptiveMessageResponseCode code, string functionName, string moduleName, Exception innerException) :
            base(message, innerException)
        {
            Code = code;
            ModuleName = moduleName;
            FunctionName = functionName;
        }

        /// <summary>
        /// Crea una instancia nueva de error especificando una excepción interna.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        /// <param name="functionName">Nombre de la función donde surge la excepción.</param>
        /// <param name="moduleName">Nombre del módulo donde surge la excepción.</param>
        /// <param name="innerException">Excepción interna causante el error actual.</param>
        public ServiceException(string message, string functionName, string moduleName, Exception innerException) :
            this(message, AdaptiveMessageResponseCode.INTERNAL_SERVER_ERROR, functionName, moduleName, innerException)
        {
        }


        /// <summary>
        /// Crea una instancia nueva de error especificando una excepción interna.
        /// </summary>
        /// <param name="functionName">Nombre de la función donde surge la excepción.</param>
        /// <param name="moduleName">Nombre del módulo donde surge la excepción.</param>
        /// <param name="innerException">Excepción interna causante el error actual.</param>
        public ServiceException(string functionName, string moduleName, Exception innerException) :
            this(null, AdaptiveMessageResponseCode.INTERNAL_SERVER_ERROR, functionName, moduleName, innerException)
        {
        }
    }
}
