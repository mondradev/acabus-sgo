using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace InnSyTech.Standard.Utils
{
    /// <summary>
    /// Ofrece funciones auxiliares para la manipulación de tipo (<see cref="Type"/>).
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Obtiene el tipo de algún tipo genérico.
        /// </summary>
        /// <param name="seqType">Tipo genético.</param>
        /// <returns>El tipo con el cual se construye el tipo genérico.</returns>
        public static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetGenericArguments()[0];
        }

        /// <summary>
        /// Obtiene el tipo del miembro especificado.
        /// </summary>
        /// <param name="member">Miembro del cual se desea obtener el tipo.</param>
        /// <returns>Instancia <see cref="Type"/> del miembro.</returns>
        public static Type GetMemberType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    break;

                case MemberTypes.Event:
                    break;

                case MemberTypes.Field:
                    return (member as FieldInfo).FieldType;

                case MemberTypes.Method:
                    break;

                case MemberTypes.Property:
                    return (member as PropertyInfo).PropertyType;

                case MemberTypes.TypeInfo:
                    return (member as Type);

                case MemberTypes.Custom:
                    break;

                case MemberTypes.NestedType:
                    break;

                case MemberTypes.All:
                    break;

                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Carga un tipo de un DLL externo.
        /// </summary>
        /// <param name="assemblyFile">Ruta del DLL externo.</param>
        /// <param name="typeName">Nombre completo del tipo a cargar.</param>
        /// <returns>Instancia <see cref="Type"/> del tipo que se ha especificado.</returns>
        public static Type LoadFromDll(String assemblyFile, String typeName)
        {
            try
            {
                if (String.IsNullOrEmpty(assemblyFile))
                    throw new ArgumentNullException(nameof(assemblyFile));

                if (String.IsNullOrEmpty(typeName))
                    throw new ArgumentNullException(nameof(typeName));

                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                return assembly.GetType(typeName);
            }
            catch (ArgumentNullException ex)
            {
                Trace.WriteLine($"No se han especificado los valores para el parametro: {ex.Message.JoinLines()}");
            }
            catch (IOException ex)
            {
                Trace.WriteLine($"Error al cargar la DLL '{assemblyFile}", "ERROR");
                Trace.WriteLine(ex.Message.JoinLines(), "ERROR");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error al cargar el tipo '{typeName}'", "ERROR");
                Trace.WriteLine(ex.Message.JoinLines(), "ERROR");
            }
            return null;
        }

        /// <summary>
        /// Busca el tipo de secuencia que está implementada en el tipo especificado.
        /// </summary>
        /// <param name="seqType">Tipo de una posible secuencia.</param>
        /// <returns>El tipo que construye la secuencia.</returns>
        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;

            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }

            return null;
        }
    }
}