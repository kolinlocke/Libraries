using Data.Ms_Sqlite.Manager;
using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Ms_Sqlite.Implementations
{
    public class Setup : Interface_Setup
    {
        public SqliteManager pCn { get; set; }

        public Setup() { this.pCn = new SqliteManager(); }

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
                .Select(O => O.Split('='))
                .ToDictionary(O => O[0], O => O[1]);

            String DbFile = Data["DbFile"];            
            String Password = Data.ContainsKey("Password") ? Data["Password"] : "";

            this.pCn = new SqliteManager(DbFile, Password);
        }
    }
}
