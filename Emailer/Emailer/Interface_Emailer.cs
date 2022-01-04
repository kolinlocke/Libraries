using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emailer
{
    public interface Interface_Emailer
    {
        EmailResponse Send_Email(EmailParams Params);
    }
}
