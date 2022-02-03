using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataQueries.Oracle.Implementations
{
    public class EntityRepository<T_Entity> : Data.Oracle.Implementations.EntityRepository<T_Entity>
        where T_Entity : class, new()
    {
        public EntityRepository(Interface_Setup Setup) : base(Setup) { }
    }
}
