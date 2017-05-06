using System;

namespace Acabus.Utils
{
 
    public class XmlAnnotationAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Ignore { get; set; }
    }
}