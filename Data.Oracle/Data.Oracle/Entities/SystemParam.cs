using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Oracle.Entities
{
    class SystemParam
    {
        public Int64 SYS_PARAM_ID { get; set; }
        public String PARAM_TYPE { get; set; }
        public String PARAM_NAME { get; set; }
        public String PARAM_VALUE { get; set; }
        public String CREA_BY { get; set; }
        public DateTime CREA_DT { get; set; }
        public String LAST_UPD_BY { get; set; }
        public DateTime? LAST_UPD_DT { get; set; }
    }
}
