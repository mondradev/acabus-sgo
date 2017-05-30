using System;

namespace InnSyTech.Standard.Database
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ColumnAttribute : Attribute
    {
        public ColumnAttribute() { }

        public Func<Object, Object> Converter { get; set; }

        public string Name { get; set; }

        public Boolean IsPrimaryKey { get; set; }

        public Boolean IsIgnored { get; set; }

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class TableAttribute : Attribute
    {
        private readonly string name;

        public TableAttribute(string name)
        {
            this.name = name;
        }

        public string Name => name;
    }
}