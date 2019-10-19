using System;

namespace Acabus.Utils
{
 
    public sealed class XmlAnnotationAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Ignore { get; set; }
    }
}