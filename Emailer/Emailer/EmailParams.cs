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
    public class EmailParams
    {
        [DataMember()]
        public String Smtp_Host { get; set; }


        [DataMember()]
        public String Sender_Email { get; set; }

        [DataMember()]
        public String Sender_Display { get; set; }

        [DataMember()]
        public List<String> Recepients_To { get; set; }

        [DataMember()]
        public List<String> Recepients_Cc { get; set; }

        [DataMember()]
        public List<String> Recepients_Bcc { get; set; }

        [DataMember()]
        public String Subject { get; set; }

        [DataMember()]
        public String Body { get; set; }

        [DataMember()]
        public Boolean IsBodyHtml { get; set; }

        [DataMember()]
        public List<String> Attachment_FilePaths { get; set; }
    }
}
