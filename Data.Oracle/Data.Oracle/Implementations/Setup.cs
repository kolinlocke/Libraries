using Commons;
using Data.Oracle.Common;
using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Oracle.Implementations
{
    public class Setup : Interface_Setup
    {
        public OracleDatabaseConnectionInstance pCn { get; set; }

        public Object Get_ConnectionData()
        {
            return this.pCn;
        }

        public void Setup_Connection(string ConnectionData)
        {
            if (String.IsNullOrEmpty(ConnectionData))
            { return; }

            Dictionary<String, String> Data =
                ConnectionData
                .Split(',')
                .Select(O => O.Split(':'))
                .ToDictionary(O => O[0], O => O[1]);

            String Host = Data["Host"];
            Int32 Port = CommonMethods.Convert_Int32(Data["Port"]);
            String ServiceID = Data["ServiceID"];
            String UserID = Data["UserID"];
            String Password = Data["Password"];

            //OracleDBHelper.InitializeConnectionString(Host, Port, ServiceID, UserID, Password);
            this.pCn = new OracleDatabaseConnectionInstance(Host, Port, ServiceID, UserID, Password);
        }
    }
}
