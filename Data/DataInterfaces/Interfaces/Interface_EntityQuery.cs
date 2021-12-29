using DataInterfaces.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataInterfaces.Interfaces
{
    public interface Interface_EntityQuery<T_Entity>
        where T_Entity : class, new()
    {
        List<T_Entity> ExecuteQuery();

        List<T_Entity> ExecuteQuery(EntityQueryParameters Parameters);
    }
}
