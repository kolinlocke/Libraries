using Data.Oracle.Common;
using Data.Oracle.Entities;
using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Oracle.Implementations
{
    public class SystemParamRepository : Interface_SystemParamRepository
    {
        Interface_Setup mSetup;

        OracleDatabaseConnectionInstance Get_Connection()
        {
            return (OracleDatabaseConnectionInstance)this.mSetup.Get_ConnectionData();
        }

        public SystemParamRepository(Interface_Setup Setup) { this.mSetup = Setup; }
        public SystemParamRepository() { this.mSetup = null;  }

        public string GetValue(string ParamType, string ParamName)
        {
            String Query =
@"
Select Tb.PARAM_VALUE
From SYSTEM_PARAM Tb
Where
    Upper(PARAM_TYPE) = :P_Type 
    And Upper(PARAM_NAME) = :P_Name
    And RowNum = 1
";

            QueryParameters Params = new QueryParameters();
            Params.Add(":P_Type", typeof(String), ParamType.ToUpper());
            Params.Add(":P_Name", typeof(String), ParamName.ToUpper());

            SystemParam QueryResult;
            if (this.mSetup == null)
                QueryResult = OracleDBHelper.ExecuteQuery<SystemParam>(Query, Params).FirstOrDefault();
            else 
                QueryResult = this.Get_Connection().ExecuteQuery<SystemParam>(Query, Params).FirstOrDefault();

            if (QueryResult != null)
            { return QueryResult.PARAM_VALUE; }
            else
            { return ""; }
        }

        public List<String> GetValues(string ParamType, string ParamName)
        {
            List<String> Result = new List<String>();

            String Query =
@"
Select Tb.PARAM_VALUE
From SYSTEM_PARAM Tb
Where
    Upper(PARAM_TYPE) = :P_Type 
    And Upper(PARAM_NAME) = :P_Name
";

            QueryParameters Params = new QueryParameters();
            Params.Add(":P_Type", typeof(String), ParamType.ToUpper());
            Params.Add(":P_Name", typeof(String), ParamName.ToUpper());

            List<SystemParam> QueryResult = new List<SystemParam>();
            if (this.mSetup == null)
                QueryResult = OracleDBHelper.ExecuteQuery<SystemParam>(Query, Params);
            else
                QueryResult = this.Get_Connection().ExecuteQuery<SystemParam>(Query, Params);

            if (QueryResult != null)
            { Result = QueryResult.Select(O => O.PARAM_VALUE).ToList(); }

            return Result;
        }

        public Int32 UpdateValue(string ParamType, string ParamName, string ParamValue)
        {
            Int32 returnValue;

            String Query =
@"
UPDATE SYSTEM_PARAM
SET PARAM_VALUE = :P_Value
WHERE 
    Upper(PARAM_TYPE) = :P_Type
    AND Upper(PARAM_NAME) = :P_Name";

            QueryParameters Params = new QueryParameters();
            Params.Add(":P_Type", typeof(String), ParamType.ToUpper());
            Params.Add(":P_Name", typeof(String), ParamName.ToUpper());
            Params.Add("P_Value", typeof(String), ParamValue.ToUpper());

            if (this.mSetup==null)
                returnValue = OracleDBHelper.ExecuteNonQuery(Query, Params);
            else 
                returnValue = this.Get_Connection().ExecuteNonQuery(Query, Params);

            return returnValue;
        }

        public DateTime GetSystemDateTime()
        {
            String Query =
@"
Select SYSDATE
From Dual
";
            SystemDate QueryResult;
            if (this.mSetup == null)
                QueryResult = OracleDBHelper.ExecuteQuery<SystemDate>(Query).FirstOrDefault();
            else 
                QueryResult = this.Get_Connection().ExecuteQuery<SystemDate>(Query).FirstOrDefault();

            return QueryResult.SYSDATE;
        }
    }
}
