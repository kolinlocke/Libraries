using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterfaces.Interfaces
{
    public interface Interface_Setup
    {
        void Setup_Connection(String ConnectionData);

        Object Get_ConnectionData();
    }
}
