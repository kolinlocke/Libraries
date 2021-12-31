using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DataFactory
{
    public interface Interface_DataFactory
    {
        void Setup();
        Interface_EntityRepository<T_Entity> Create_EntityRepository<T_Entity>() where T_Entity : class, new();
        Interface_SystemParamRepository Create_SystemParamRepository();
        Interface_EntityQuery<T_Entity> Create_EntityQuery<T_Entity>(String QueryName) where T_Entity : class, new();
    }
}
