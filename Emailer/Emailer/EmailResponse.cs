using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Emailer
{
    [Serializable()]
    [DataContract()]
    public class EmailResponse
    {
        [DataMember]
        public SimpleException SimpleException { get; set; }
            
        [DataMember]
        public String ExceptionMsg { get; set; }

        [DataMember]
        public Boolean Result { get; set; }
    }
}
