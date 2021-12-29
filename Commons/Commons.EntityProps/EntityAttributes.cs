using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Commons.EntityProps.Common_Enums;

namespace Commons.EntityProps
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EntityAttribute : Attribute
    {
        public String EntityName { get; set; }
        public String QueryName { get; set; }
        public EntitySource EntitySource { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EntityFieldAttribute : Attribute
    {
        public String FieldName { get; set; }
        public Boolean IsKey { get; set; }
    }
}