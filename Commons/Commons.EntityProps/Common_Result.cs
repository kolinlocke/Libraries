using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.EntityProps
{
    public class Common_Result
    {
        public class Result_Ct
        {
            public Int32 CT { get; set; }
        }

        public class Result_String
        {
            public String VALUE { get; set; }
        }

        public class Result_DateTime
        {
            public DateTime VALUE { get; set; }
        }

        public class Result_Int32
        {
            public Int32 VALUE { get; set; }
        }

        public class Result_Int64
        {
            public Int64 VALUE { get; set; }
        }

        public class Result_Double
        {
            public Double VALUE { get; set; }
        }

        public class Result_Boolean
        {
            public Boolean VALUE { get; set; }
        }

        public class Result_Value<T>
        {
            public T VALUE { get; set; }
        }
    }
}
