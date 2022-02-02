using Data.DataFactory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Data.DataFactory.DataFactory;

namespace Data.Oracle.Sample_DataFactory
{
    public class Oracle_DataFactory : DataFactoryBase
    {
        String mConnectionData;

        public Oracle_DataFactory(String ConnectionData)
        {
            this.mConnectionData = ConnectionData;
        }

        public override DataFactory.DataFactory.SetupParams Setup_Params()
        {
            SetupParams DataFactory_SetupParams = new SetupParams();
            DataFactory_SetupParams.FactoryName = "DataFactory_Fits";
            //DataFactory_SetupParams.AssemblyFilePath = ConfigurationManager.AppSettings["DataFactory_AssemblyFilePath"];
            DataFactory_SetupParams.AssemblySourceType = typeof(Data.Oracle.Implementations.Setup);
            DataFactory_SetupParams.ClassName_Setup = "Implementations.Setup";
            DataFactory_SetupParams.ClassName_EntityRepository = "Implementations.EntityRepository";
            DataFactory_SetupParams.ClassName_SystemParamRepository = "Implementations.SystemParamRepository";
            DataFactory_SetupParams.ConnectionData = this.mConnectionData;

            return DataFactory_SetupParams;
        }
    }
}
