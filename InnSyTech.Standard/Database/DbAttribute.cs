﻿using System;

namespace InnSyTech.Standard.Database
{

    /// <summary>
    /// Representa un atributo que permite indicar como se maneja la propiedad de una estructura dentro de la base de datos.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Crea una instancia nueva de un atributo Columna.
        /// </summary>
        public ColumnAttribute() { }

        /// <summary>
        /// Obtiene o establece el convertidor del valor de la propiedad.
        /// </summary>
        public Type Converter { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre de la llave foranea de una relación de uno a varios.
        /// </summary>
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// Obtiene o establece si el campo es autoincremental.
        /// </summary>
        public Boolean IsAutonumerical { get; set; }

        /// <summary>
        /// Obtiene o establece si el campo es una llave foranea a otra tabla.
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Obtiene o establece si la propiedad debe ser ignorada en la base de datos.
        /// </summary>
        public Boolean IsIgnored { get; set; }

        /// <summary>
        /// Obtiene o establece si la propiedad es una llave primaria dentro de la base de datos.
        /// </summary>
        public Boolean IsPrimaryKey { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre de la propiedad dentro de la base de datos.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Representa un atributo que indica algunas caracteristicas de una tabla dentro de una base de datos.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class EntityAttribute : Attribute
    {
        /// <summary>
        /// Crea una instancia nueva de un atributo Tabla.
        /// </summary>
        public EntityAttribute() { }

        /// <summary>
        /// Obtiene el nombre de la tabla.
        /// </summary>
        public string TableName { get; set; }
    }

}