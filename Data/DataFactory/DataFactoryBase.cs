using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Data.DataFactory.DataFactory;

namespace Data.DataFactory
{
    public abstract class DataFactoryBase : Interface_DataFactory
    {
        DataFactory_Instance mDf_Instance;

        public abstract SetupParams Setup_Params();

        public DataFactoryBase() { }

        public void Setup()
        {
            var Params = this.Setup_Params();
            this.mDf_Instance = new DataFactory_Instance(Params);
        }

        public Interface_EntityQuery<T_Entity> Create_EntityQuery<T_Entity>(string QueryName) where T_Entity : class, new()
        {
            return this.mDf_Instance.Create_EntityQuery<T_Entity>(QueryName);
        }

        public Interface_EntityRepository<T_Entity> Create_EntityRepository<T_Entity>() where T_Entity : class, new()
        {
            return this.mDf_Instance.Create_EntityRepository<T_Entity>();
        }

        public Interface_SystemParamRepository Create_SystemParamRepository()
        {
            return this.mDf_Instance.Create_SystemParamRepository();
        }
    }
}
