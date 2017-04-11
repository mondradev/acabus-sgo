using System;

namespace ACABUS_Control_de_operacion.Utils
{
 
    public class XmlAnnotationAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Ignore { get; set; }
    }
}