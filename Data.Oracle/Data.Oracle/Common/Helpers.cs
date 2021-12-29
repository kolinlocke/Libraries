using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataInterfaces.Common;
using Oracle.ManagedDataAccess.Client;

namespace Data.Oracle.Common
{
    public class Helpers
    {
        public static List<T_Entity> ExecuteQuery<T_Entity>(String Query, EntityQueryParameters Parameters, OracleDatabaseConnectionInstance Cn)
        where T_Entity : class, new()
        {
            QueryParameters Params = new QueryParameters();
            if (Parameters != null)
            {
                Parameters.ForEach(O =>
                {
                    Params.Add(String.Format(":{0}", O.ParameterName), O.ParameterType, O.ParameterValue);
                });
            }

            var Retrieved = Cn.ExecuteQuery<T_Entity>(Query, Params);
            return Retrieved;
        }

        public static List<T_Entity> ExecuteQuery<T_Entity>(String Query, EntityQueryParameters Parameters, CommandType Type, OracleDatabaseConnectionInstance Cn)
            where T_Entity : class, new()
        {
            List<OracleParameter> Params = new List<OracleParameter>();
            if (Parameters != null)
            {
                Params = Parameters.Select(O =>
                          new OracleParameter()
                          {
                              ParameterName = O.ParameterName,
                              Value = O.ParameterValue,
                              OracleDbType = OracleDBHelper.ConvertTypeToOracleType(O.ParameterType),
                              DbType = OracleDBHelper.ConvertTypeToDbType(O.ParameterType),
                              Direction = O.ParameterDirection
                          }).ToList();

            }

            var Retrieved = Cn.ExecuteQuery<T_Entity>(Query, Params, Type);
            return Retrieved;
        }

        public static Int32 ExecuteNonQuery(String Query, EntityQueryParameters Parameters, CommandType Type, OracleDatabaseConnectionInstance Cn)
        {
            QueryParameters Params = new QueryParameters();
            if (Parameters != null)
            {
                Parameters.ForEach(O =>
                {
                    Params.Add(
                        String.Format("{0}", O.ParameterName)
                        , O.ParameterType
                        , O.ParameterValue
                        , O.ParameterDirection);
                });
            }

            var Returned = Cn.ExecuteNonQuery(Query, Params, Type: Type);

            Params.ForEach(O =>
            {
                Parameters[O.Name].ParameterValue = O.Value;
            });

            return Returned;
        }

        public static List<T_Entity> ExecuteQueryWithParams<T_Entity>(String Query, EntityQueryParameters Parameters, OracleDatabaseConnectionInstance Cn)
            where T_Entity : class, new()
        {
            List<String> Field_Params = new List<String>();
            QueryParameters Params = new QueryParameters();
            if (Parameters != null)
            {
                Parameters.ForEach(O =>
                {
                    String FieldName = O.ParameterName;
                    String ParameterName = String.Format(":P_{0}", Parameters.IndexOf(O));
                    Field_Params.Add(String.Format("Tb.{0} = :{1}", FieldName, ParameterName));
                    Params.Add(ParameterName, O.ParameterType, O.ParameterValue);
                });
            }

            String Query_Params = "";
            if (Field_Params.Count > 0)
            {
                Query_Params = String.Join(" And ", Field_Params);
            }

            String Query_Parameterized =
@"
Select Tb.*
From ({0}) Tb
Where
    1 = 1
    {1}
";

            Query_Parameterized =
                String.Format(Query_Parameterized, Query, Query_Params);

            var Retrieved = Cn.ExecuteQuery<T_Entity>(Query_Parameterized, Params);
            return Retrieved;
        }

        //[-]

        public static List<T_Entity> ExecuteQuery<T_Entity>(String Query, EntityQueryParameters Parameters)
            where T_Entity : class, new()
        {
            QueryParameters Params = new QueryParameters();
            if (Parameters != null)
            {
                Parameters.ForEach(O =>
                {
                    Params.Add(String.Format(":{0}", O.ParameterName), O.ParameterType, O.ParameterValue);
                });
            }

            var Retrieved = OracleDBHelper.ExecuteQuery<T_Entity>(Query, Params);
            return Retrieved;
        }

        public static List<T_Entity> ExecuteQuery<T_Entity>(String Query, EntityQueryParameters Parameters, CommandType Type)
            where T_Entity : class, new()
        {
            QueryParameters Params = new QueryParameters();
            if (Parameters != null)
            {
                Parameters.ForEach(O =>
                {
                    Params.Add(String.Format(":{0}", O.ParameterName), O.ParameterType, O.ParameterValue);
                });
            }

            var Retrieved = OracleDBHelper.ExecuteQuery<T_Entity>(Query, Params);
            return Retrieved;
        }

        public static Int32 ExecuteNonQuery(String Query, EntityQueryParameters Parameters, CommandType Type)
        {
            QueryParameters Params = new QueryParameters();
            if (Parameters != null)
            {
                Parameters.ForEach(O =>
                {
                    Params.Add(
                        String.Format("{0}", O.ParameterName)
                        , O.ParameterType
                        , O.ParameterValue
                        , O.ParameterDirection);
                });
            }

            var Returned = OracleDBHelper.ExecuteNonQuery(Query, Params, QueryType: Type);

            Params.ForEach(O =>
            {
                Parameters[O.Name].ParameterValue = O.Value;
            });

            return Returned;
        }

        public static List<T_Entity> ExecuteQueryWithParams<T_Entity>(String Query, EntityQueryParameters Parameters)
            where T_Entity : class, new()
        {
            List<String> Field_Params = new List<String>();
            QueryParameters Params = new QueryParameters();
            if (Parameters != null)
            {
                Parameters.ForEach(O =>
                {
                    String FieldName = O.ParameterName;
                    String ParameterName = String.Format(":P_{0}", Parameters.IndexOf(O));
                    Field_Params.Add(String.Format("Tb.{0} = :{1}", FieldName, ParameterName));
                    Params.Add(ParameterName, O.ParameterType, O.ParameterValue);
                });
            }

            String Query_Params = "";
            if (Field_Params.Count > 0)
            {
                Query_Params = String.Join(" And ", Field_Params);
            }

            String Query_Parameterized =
@"
Select Tb.*
From ({0}) Tb
Where
    1 = 1
    {1}
";

            Query_Parameterized =
                String.Format(Query_Parameterized, Query, Query_Params);

            var Retrieved = OracleDBHelper.ExecuteQuery<T_Entity>(Query_Parameterized, Params);
            return Retrieved;
        }
    }
}
