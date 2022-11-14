using Commons;
using Data.SQLServer.Common;
using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.SQLServer.Implementations
{
    public class Setup : Interface_Setup
    {
        public SQLServerManager pCn { get; set; }

        public object Get_ConnectionData()
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

            String Server = Data["Server"];
            //Int32 Port = CommonMethods.Convert_Int32(Data["Port"]);
            String Database = Data["Database"];
            String UserID = Data["UserID"];
            String Password = Data["Password"];

            this.pCn =
                new SQLServerManager(
                    Server
                    , Database
                    , UserID
                    , Password);
        }
    }
}
