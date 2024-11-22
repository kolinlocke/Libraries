using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.EntityProps
{
    public class EntityPredicate
    {
        public String FieldName { get; set; }

        public Object FieldValue { get; set; }

        public Type FieldType { get; set; }

        public List<Object> FieldValueList { get; set; }
    }
}
