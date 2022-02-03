using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataInterfaces.Interfaces;


namespace DataQueries.Oracle.Implementations
{
    public class SystemParamRepository : Data.Oracle.Implementations.SystemParamRepository
    {
        public SystemParamRepository(Interface_Setup Setup) : base(Setup) { }
    }
}
