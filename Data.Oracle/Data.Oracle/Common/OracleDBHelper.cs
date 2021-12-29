using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;
using static Data.Oracle.Common.OracleDBHelper;
using Commons;
using Commons.EntityProps;

namespace Data.Oracle.Common
{
    #region _OracleDBHelper

    public class OracleDBHelper
    {
        #region _Definitions

        public class TableDef : List<TableDef_Fields> { }

        public class TableDef_Fields
        {
            public String FieldName { get; set; }
            public Type FieldType { get; set; }
            public Int32 Length { get; set; }
            public Int32 Precision { get; set; }
            public Int32 Scale { get; set; }
            public Boolean Is_Nullable { get; set; }
            public Boolean Is_PK { get; set; }
            public Boolean Is_Identity { get; set; }
            public String DbType { get; set; }
            public Int32 FieldCt { get; set; }
        }

        public enum eSaveDataRow_Process
        {
            Process_Insert,
            Process_Update,
            Process_Delete
        }

        public enum eSaveData_Process
        {
            Process_Insert,
            //Process_InsertDefault,
            Process_Update
        }

        protected struct Prepare_SaveDataRow_Returned
        {
            public OracleCommand Cmd;
            public Boolean Result;
            public String TableName;
            public TableDef TableDef;
            public Prepare_SaveDataRow_Returned_Insert Insert_Params;
            public Prepare_SaveDataRow_Returned_Update Update_Params;
            public Prepare_SaveDataRow_Returned_Delete Delete_Params;
        }

        protected struct Prepare_SaveDataRow_Returned_Insert
        {
            public List<String> Query_Insert_Params;
            public List<String> Query_Insert_Returning_Params;
        }

        protected struct Prepare_SaveDataRow_Returned_Update
        {
            public List<String> Query_Update_Params;
            public List<String> Query_Update_Where_Params;
        }

        protected struct Prepare_SaveDataRow_Returned_Delete
        {
            public List<String> Query_Delete_Where_Params;
        }

        public class EntityKeys : List<EntityKey> { }

        public class EntityKey
        {
            public String Name { get; set; }
            public Object Value { get; set; }
            public Type Type { get; set; }
            public Boolean Is_Identity { get; set; }
        }

        #endregion

        #region _Variables

        static Dictionary<Type, DbType> mDbTypeMap = null;

        #endregion

        #region _Properties

        /// <summary>
        /// Get or set the connection string to be used by query builder
        /// </summary>
        public static string ConnectionString { get; set; }

        #endregion

        #region _Constructor

        static OracleDBHelper()
        {
            Setup_DbTypeMap();
        }

        static void Setup_DbTypeMap()
        {
            mDbTypeMap = new Dictionary<Type, DbType>();
            mDbTypeMap[typeof(byte)] = DbType.Byte;
            mDbTypeMap[typeof(sbyte)] = DbType.SByte;
            mDbTypeMap[typeof(short)] = DbType.Int16;
            mDbTypeMap[typeof(ushort)] = DbType.UInt16;
            mDbTypeMap[typeof(int)] = DbType.Int32;
            mDbTypeMap[typeof(uint)] = DbType.UInt32;
            mDbTypeMap[typeof(long)] = DbType.Int64;
            mDbTypeMap[typeof(ulong)] = DbType.UInt64;
            mDbTypeMap[typeof(float)] = DbType.Single;
            mDbTypeMap[typeof(double)] = DbType.Double;
            mDbTypeMap[typeof(decimal)] = DbType.Decimal;
            mDbTypeMap[typeof(bool)] = DbType.Boolean;
            mDbTypeMap[typeof(string)] = DbType.String;
            mDbTypeMap[typeof(char)] = DbType.StringFixedLength;
            mDbTypeMap[typeof(Guid)] = DbType.Guid;
            mDbTypeMap[typeof(DateTime)] = DbType.DateTime;
            mDbTypeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            mDbTypeMap[typeof(byte[])] = DbType.Binary;
            mDbTypeMap[typeof(byte?)] = DbType.Byte;
            mDbTypeMap[typeof(sbyte?)] = DbType.SByte;
            mDbTypeMap[typeof(short?)] = DbType.Int16;
            mDbTypeMap[typeof(ushort?)] = DbType.UInt16;
            mDbTypeMap[typeof(int?)] = DbType.Int32;
            mDbTypeMap[typeof(uint?)] = DbType.UInt32;
            mDbTypeMap[typeof(long?)] = DbType.Int64;
            mDbTypeMap[typeof(ulong?)] = DbType.UInt64;
            mDbTypeMap[typeof(float?)] = DbType.Single;
            mDbTypeMap[typeof(double?)] = DbType.Double;
            mDbTypeMap[typeof(decimal?)] = DbType.Decimal;
            mDbTypeMap[typeof(bool?)] = DbType.Boolean;
            mDbTypeMap[typeof(char?)] = DbType.StringFixedLength;
            mDbTypeMap[typeof(Guid?)] = DbType.Guid;
            mDbTypeMap[typeof(DateTime?)] = DbType.DateTime;
            mDbTypeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
        }

        #endregion

        #region _Methods

        public static void InitializeConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static void InitializeConnectionString(
            String Host
            , Int32 Port
            , String ServiceID
            , String UserID
            , String Password)
        {
            String ConnectionStringTemplate = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SID={2})));User Id={3};Password={4};";
            String ConnectionString =
                String.Format(
                ConnectionStringTemplate
                , Host
                , Port
                , ServiceID
                , UserID
                , Password);

            InitializeConnectionString(ConnectionString);
        }

        /// <summary>
        ///DirkFF04122016 : Optimization - set session back to its default settings 
        ///                  which the library (used to create connections) alters its value to case insensitive 
        ///                  and render all conventional indexes useless.
        /// </summary>
        /// <param name="objConn">
        /// </param>
        private static void SetDBSessionProperties(OracleConnection objConn)
        {
            using (var objCmd = new OracleCommand())
            {
                objCmd.Connection = objConn;

                if (objConn.State != ConnectionState.Open)
                {
                    try
                    { objConn.Open(); } //Jhun Dacera 11042015 - Added catcher to handle exception due to optimization in SO Detail
                    catch { }
                }

                objCmd.CommandText = @"BEGIN EXECUTE IMMEDIATE 'ALTER SESSION SET NLS_COMP=BINARY'; EXECUTE IMMEDIATE 'ALTER SESSION SET NLS_COMP=BINARY'; END;";
                objCmd.CommandType = CommandType.Text;

                objCmd.ExecuteNonQuery();
            }
        }

        public static OracleConnection CreateConnection()
        { return new OracleConnection(ConnectionString); }

        public static OracleCommand PrepareQuery(String Query, QueryParameters Params, OracleConnection Cn = null, CommandType QueryType = CommandType.Text)
        {
            List<OracleParameter> OracleParams =
                Params.Select(O =>
                    new OracleParameter()
                    {
                        ParameterName = O.Name,
                        Value = O.Value,
                        OracleDbType = OracleDBHelper.ConvertTypeToOracleType(O.Type)
                    }).ToList();

            return PrepareQuery(Query, OracleParams, Cn, QueryType);
        }

        public static OracleCommand PrepareQuery(String Query, List<OracleParameter> Params, OracleConnection Cn = null, CommandType QueryType = CommandType.Text)
        {
            //OracleConnection Cn = new OracleConnection(ConnectionString);
            if (Cn == null)
            {
                Cn = new OracleConnection(ConnectionString);
                Cn.Open();
            }

            OracleCommand Cmd = new OracleCommand();
            Cmd.BindByName = true;
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Params.ForEach(O => Cmd.Parameters.Add(O));
            Cmd.CommandType = QueryType;
            Cmd.Prepare();

            return Cmd;
        }

        #endregion

        #region _Methods.Execute

        public static OracleDataReader ExecuteReader(string query)
        {
            var con = new OracleConnection(ConnectionString);
            try
            {
                con.Open();

                //SetDBSessionProperties(con);

                var oraCommand = new OracleCommand(query);
                return oraCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            }
            catch
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
                //Log the error
                //Logger.Logger.LogInfo(ex.Message);
                return null;
            }
        }

        public static DataTable ExecuteQuery(string query)
        {
            //var con = new OracleConnection(ConnectionString);
            //OracleDataAdapter da = new OracleDataAdapter(query, con);
            //DataSet ds = new DataSet();
            //try
            //{
            //    con.Open();

            //    SetDBSessionProperties(con);

            //    da.Fill(ds);
            //    con.Close();
            //    return ds.Tables[0];
            //}
            //catch
            //{
            //    if (con.State == System.Data.ConnectionState.Open)
            //        con.Close();
            //    return null;
            //}

            return ExecuteQuery(query, (QueryParameters)null);
        }

        public static DataTable ExecuteQuery(String Query, QueryParameters Params, OracleConnection Cn = null)
        {
            List<OracleParameter> OracleParams =
                (Params ?? new QueryParameters()).Select(O =>
                    new OracleParameter()
                    {
                        ParameterName = O.Name,
                        Value = O.Value,
                        OracleDbType = OracleDBHelper.ConvertTypeToOracleType(O.Type)
                    }).ToList();
            return ExecuteQuery(Query, OracleParams, Cn);
        }

        public static DataTable ExecuteQuery(String Query, List<OracleParameter> Params, OracleConnection Cn = null)
        {
            return ExecuteQuery(Query, Params, CommandType.Text, Cn);
        }

        public static DataTable ExecuteQuery(String Query, List<OracleParameter> Params, CommandType Type, OracleConnection Cn = null)
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new OracleConnection(ConnectionString);
                Cn.Open();
            }

            //SetDBSessionProperties(Cn);

            OracleCommand Cmd = new OracleCommand();
            Cmd.BindByName = true;
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Cmd.CommandType = Type;

            if (Params != null)
            { Params.ForEach(O => Cmd.Parameters.Add(O)); }

            DataSet Ds_QueryResult = new DataSet();
            OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
            Adp.Fill(Ds_QueryResult);

            if (Is_Cn)
            { Cn.Close(); }

            return Ds_QueryResult.Tables[0];
        }

        public static DataTable ExecuteQuery(OracleCommand Cmd)
        {
            DataSet Ds_QueryResult = new DataSet();
            OracleDataAdapter Adp = new OracleDataAdapter(Cmd);
            Adp.Fill(Ds_QueryResult);

            return Ds_QueryResult.Tables[0];
        }

        public static List<T> ExecuteQuery<T>(String Query, QueryParameters Params = null, OracleConnection Cn = null) where T : class, new()
        {
            List<OracleParameter> OracleParams =
              (Params ?? new QueryParameters()).Select(O =>
                  new OracleParameter()
                  {
                      ParameterName = O.Name,
                      Value = O.Value,
                      OracleDbType = OracleDBHelper.ConvertTypeToOracleType(O.Type),
                      Direction = ParameterDirection.Input                      
                  }).ToList();

            return ExecuteQuery<T>(Query, OracleParams, CommandType.Text, Cn);
        }

        public static List<T> ExecuteQuery<T>(String Query, List<OracleParameter> Params) where T : class, new()
        {
            return ExecuteQuery<T>(Query, Params, CommandType.Text, null);
        }

        public static List<T> ExecuteQuery<T>(String Query, List<OracleParameter> Params, CommandType Type, OracleConnection Cn = null) where T : class, new()
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new OracleConnection(ConnectionString);
                Cn.Open();
            }

            //SetDBSessionProperties(Cn);

            OracleCommand Cmd = new OracleCommand();
            Cmd.BindByName = true;
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Cmd.CommandType = Type;

            if (Params != null)
            { Params.ForEach(O => Cmd.Parameters.Add(O.Clone())); }

            List<T> Returned = ExecuteQuery<T>(Cmd);

            if (Is_Cn)
            { Cn.Close(); }

            return Returned;
        }

        public static List<T> ExecuteQuery<T>(OracleCommand Cmd) where T : class, new()
        {
            /*
            var Dt_Data = ExecuteQuery(Cmd);
            var T_Data = DBMappingHelper.CreateListFromDataTable<T>(Dt_Data);
            //return T_Data;
            */

            var Reader = Cmd.ExecuteReader(CommandBehavior.CloseConnection);
            var Schema = Reader.GetSchemaTable();

            List<T> T_List = new List<T>();
            while (Reader.Read())
            {
                T T_Data = new T();
                T_List.Add(T_Data);

                foreach (DataRow Row in Schema.Rows)
                {
                    String ColumnName = CommonMethods.Convert_String(Row["ColumnName"]);
                    Int32 ColumnOrdinal = CommonMethods.Convert_Int32(Row["ColumnOrdinal"]);
                    //T_Data.SetPropertyValue(ColumnName, Reader[ColumnOrdinal]);
                    T_Data.Set_EntityValue(ColumnName, Reader[ColumnOrdinal]);
                }
            }

            return T_List;
        }

        public static Int32 ExecuteNonQuery(String Query)
        {
            return ExecuteNonQuery(Query, (QueryParameters)null);
        }

        public static Int32 ExecuteNonQuery(String Query, QueryParameters Params, OracleConnection Cn = null, CommandType QueryType = CommandType.Text)
        {
            List<OracleParameter> OracleParams =
                (Params ?? new QueryParameters()).Select(O =>
                    new OracleParameter()
                    {
                        ParameterName = O.Name,
                        Value = O.Value,
                        OracleDbType = OracleDBHelper.ConvertTypeToOracleType(O.Type),
                        DbType = OracleDBHelper.ConvertTypeToDbType(O.Type),
                        Direction = O.Direction
                    }).ToList();

            var Returned = ExecuteNonQuery(Query, OracleParams, Cn, QueryType);

            OracleParams.ForEach(O_Op =>
            {
                Params.FirstOrDefault(O => O.Name == O_Op.ParameterName).Value = O_Op.Value;
            });

            return Returned;
        }

        public static Int32 ExecuteNonQuery(String Query, List<OracleParameter> Params, OracleConnection Cn = null, CommandType QueryType = CommandType.Text)
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new OracleConnection(ConnectionString);
                Cn.Open();
            }

            //SetDBSessionProperties(Cn);

            OracleCommand Cmd = new OracleCommand();
            Cmd.BindByName = true;
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Cmd.CommandType = QueryType;
            Params.ForEach(O => Cmd.Parameters.Add(O));

            var ExecuteResult = Cmd.ExecuteNonQuery();

            if (Is_Cn)
            { Cn.Close(); }

            return ExecuteResult;
        }

        public static void ExecuteNonQueryStoredProcedure(String Query, List<OracleParameter> Params, OracleConnection Cn = null)
        {
            ExecuteNonQuery(Query, Params, Cn, CommandType.StoredProcedure);
        }

        public static object ExecuteQueryAggregate(String Query, OracleConnection Cn = null, CommandType QueryType = CommandType.Text)
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new OracleConnection(ConnectionString);
                Cn.Open();
            }

            //SetDBSessionProperties(Cn);

            OracleCommand Cmd = new OracleCommand();
            Cmd.BindByName = true;
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Cmd.CommandType = QueryType;

            var ExecuteResult = Cmd.ExecuteScalar();

            if (Is_Cn)
            { Cn.Close(); }

            return ExecuteResult;
        }

        public static DataTable ExecuteStoredProcedure(String Query, List<OracleParameter> Params, OracleConnection Cn = null)
        {
            return ExecuteQuery(Query, Params, CommandType.StoredProcedure, Cn);
        }

        #endregion

        #region _Methods.SaveData

        /// <summary>
        /// Used in Ev_SaveData_ExecuteSaveDataRow
        /// </summary>
        /// <param name="RowCt">
        /// Current Row executed
        /// </param>
        /// <param name="RowTotal">
        /// Total Rows in total
        /// </param>
        public delegate void Ds_SaveData_ExecuteSaveDataRow_Handler(Int32 RowCt, Int32 RowTotal);

        /// <summary>
        /// Static Event! Be careful when using this! Unsubscribe event handlers when done using this!
        /// </summary>
        public static event Ds_SaveData_ExecuteSaveDataRow_Handler Ev_SaveData_ExecuteSaveDataRow;

        public static Boolean SaveData(
            eSaveData_Process ProcessType
            , DataTable Data
            , String TableName = ""
            , OracleConnection Cn = null
            , EntityKeys Keys = null)
        {
            if (!Check_TableExists(TableName, Cn))
            { throw new Exception($"Oracle Table {TableName} doesn't exist"); }

            Boolean Is_Cn = Cn != null;

            eSaveDataRow_Process SaveDataRowProcessType = eSaveDataRow_Process.Process_Insert;
            switch (ProcessType)
            {
                case eSaveData_Process.Process_Insert:
                    SaveDataRowProcessType = eSaveDataRow_Process.Process_Insert;
                    break;
                case eSaveData_Process.Process_Update:
                    SaveDataRowProcessType = eSaveDataRow_Process.Process_Update;
                    break;
            }

            var Prepared =
                Prepare_SaveDataRow(
                    SaveDataRowProcessType
                    , TableName
                    , Data
                    , Cn
                    , Keys);

            Int32 RowCt = 0;
            foreach (DataRow Item_DataRow in Data.Rows)
            {
                Execute_SaveDataRow(SaveDataRowProcessType, Prepared, Item_DataRow);
                RowCt++;
                if (Ev_SaveData_ExecuteSaveDataRow != null)
                { Ev_SaveData_ExecuteSaveDataRow(RowCt, Data.Rows.Count); }
            }

            //If Connection is not supplied, close the connection in the command obj
            if (!Is_Cn)
            { Prepared.Cmd.Connection.Close(); }

            return true;
        }

        static Prepare_SaveDataRow_Returned Prepare_SaveDataRow(
            eSaveDataRow_Process SaveDataRow_Process
            , String TableName
            , DataTable Data
            , OracleConnection Cn = null
            , EntityKeys Keys = null)
        {
            Prepare_SaveDataRow_Returned Returned =
                new Prepare_SaveDataRow_Returned() { Cmd = null, Result = false };

            //Get Table Column Definition
            TableDef TableDef = Get_TableDef(TableName, Cn);

            //If Param Keys is present, replace the keys in TableDef
            if (Keys != null)
            {
                TableDef.ForEach(O =>
                {
                    O.Is_PK = false;
                    var Key = Keys.FirstOrDefault(O_Key => O_Key.Name == O.FieldName);
                    if (Key != null)
                    {
                        O.Is_PK = true;
                        O.Is_Identity = Key.Is_Identity;
                    }
                });
            }

            Returned.TableName = TableName;
            Returned.TableDef = TableDef;

            List<String> Entity_Fields = new List<String>();
            foreach (DataColumn Item_Column in Data.Columns)
            { Entity_Fields.Add(Item_Column.ColumnName); }

            switch (SaveDataRow_Process)
            {
                case eSaveDataRow_Process.Process_Insert:
                    {
                        Returned = Prepare_SaveDataRow_Insert(Returned, Entity_Fields, Cn);
                        break;
                    }
                case eSaveDataRow_Process.Process_Update:
                    {
                        Returned = Prepare_SaveDataRow_Update(Returned, Entity_Fields, Cn);
                        break;
                    }
                case eSaveDataRow_Process.Process_Delete:
                    {
                        Returned = Prepare_SaveDataRow_Delete(Returned, Cn);
                        break;
                    }
            }

            return Returned;
        }

        static Prepare_SaveDataRow_Returned Prepare_SaveDataRow_Insert(
            Prepare_SaveDataRow_Returned Returned
            , List<String> Entity_Fields
            , OracleConnection Cn = null)
        {
            String TableName = Returned.TableName;
            TableDef TableDef = Returned.TableDef;

            List<OracleParameter> Params = new List<OracleParameter>();
            List<String> Query_Insert_Fields = new List<String>();
            List<String> Query_Insert_Field_Params = new List<String>();

            //Add all non PK fields to Params
            foreach (var Item_Def in TableDef.ToList().Where(O => (!O.Is_PK || (O.Is_PK && !O.Is_Identity)) && Entity_Fields.Contains(O.FieldName)))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);

                Query_Insert_Fields.Add(Item_Def.FieldName);
                Query_Insert_Field_Params.Add(ParamName);

                OracleParameter Param = new OracleParameter();
                Param.ParameterName = ParamName;
                Param.OracleDbType = GetOracleType(Item_Def.DbType);
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            List<String> Query_Returning_Fields = new List<String>();
            List<String> Query_Returning_Params = new List<String>();

            //Process PK Fields for output
            foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK && O.Is_Identity))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);

                Query_Returning_Fields.Add(Item_Def.FieldName);
                Query_Returning_Params.Add(ParamName);

                OracleParameter Param = new OracleParameter();
                Param.ParameterName = ParamName;
                Param.OracleDbType = GetOracleType(Item_Def.DbType);
                Param.Direction = ParameterDirection.Output;
                Params.Add(Param);
            }

            String Query =
@"
Insert Into " + TableName + @"
(" + String.Join(" , ", Query_Insert_Fields) + @")
Values
(" + String.Join(" , ", Query_Insert_Field_Params) + @")
";

            String Query_Returning = "";
            if (Query_Returning_Fields.Any())
            {
                Query_Returning =
@"
Returning
" + String.Join(" , ", Query_Returning_Fields) + @"
Into
" + String.Join(" , ", Query_Returning_Params) + @"
";
            }

            Query = Query + Query_Returning;

            Returned.Cmd = OracleDBHelper.PrepareQuery(Query, Params, Cn);
            Returned.Insert_Params.Query_Insert_Params = Query_Insert_Field_Params;
            Returned.Insert_Params.Query_Insert_Returning_Params = Query_Returning_Params;

            return Returned;
        }

        static Prepare_SaveDataRow_Returned Prepare_SaveDataRow_Update(
            Prepare_SaveDataRow_Returned Returned
            , List<String> Entity_Fields
            , OracleConnection Cn = null)
        {
            String TableName = Returned.TableName;
            TableDef TableDef = Returned.TableDef;

            List<OracleParameter> Params = new List<OracleParameter>();
            List<String> Query_Update_Fields = new List<String>();
            List<String> Query_Update_Field_Params = new List<String>();
            List<String> Query_Update_Where = new List<String>();
            List<String> Query_Update_Where_Params = new List<String>();

            //Add all non PK fields to Params
            foreach (var Item_Def in TableDef.ToList().Where(O => !O.Is_PK && Entity_Fields.Contains(O.FieldName)))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);

                String Query_Update_Field = String.Format(" {0} = {1} ", Item_Def.FieldName, ParamName);

                Query_Update_Fields.Add(Query_Update_Field);
                Query_Update_Field_Params.Add(ParamName);

                OracleParameter Param = new OracleParameter();
                Param.ParameterName = ParamName;
                Param.OracleDbType = GetOracleType(Item_Def.DbType);
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            //Process PK Fields for Where Clause
            foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);
                String Query_Where = String.Format(" {0} = {1} ", Item_Def.FieldName, ParamName);

                Query_Update_Where.Add(Query_Where);
                Query_Update_Where_Params.Add(ParamName);

                OracleParameter Param = new OracleParameter();
                Param.ParameterName = ParamName;
                Param.OracleDbType = GetOracleType(Item_Def.DbType);
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            String Query =
@"
Update " + TableName + @"
Set
" + String.Join(" , ", Query_Update_Fields) + @"
Where
" + String.Join(" And ", Query_Update_Where) + @"
";

            Returned.Cmd = OracleDBHelper.PrepareQuery(Query, Params, Cn);
            Returned.Update_Params.Query_Update_Params = Query_Update_Field_Params;
            Returned.Update_Params.Query_Update_Where_Params = Query_Update_Where_Params;

            return Returned;
        }

        static Prepare_SaveDataRow_Returned Prepare_SaveDataRow_Delete(Prepare_SaveDataRow_Returned Returned, OracleConnection Cn = null)
        {
            String TableName = Returned.TableName;
            TableDef TableDef = Returned.TableDef;

            List<OracleParameter> Params = new List<OracleParameter>();
            List<String> Query_Update_Where = new List<String>();
            List<String> Query_Delete_Where_Params = new List<String>();

            //Process PK Fields for Where Clause
            foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);
                String Query_Where = String.Format(" {0} = {1} ", Item_Def.FieldName, ParamName);

                Query_Update_Where.Add(Query_Where);
                Query_Delete_Where_Params.Add(ParamName);

                OracleParameter Param = new OracleParameter();
                Param.ParameterName = ParamName;
                Param.OracleDbType = GetOracleType(Item_Def.DbType);
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            String Query =
@"
Delete From " + TableName + @"
Where
" + String.Join(" And ", Query_Update_Where) + @"
";

            Returned.Cmd = OracleDBHelper.PrepareQuery(Query, Params, Cn);
            Returned.Delete_Params.Query_Delete_Where_Params = Query_Delete_Where_Params;

            return Returned;
        }

        static void Execute_SaveDataRow(eSaveDataRow_Process SaveDataRow_Process, Prepare_SaveDataRow_Returned Prepared, DataRow Data)
        {
            //Execute Prepared Query
            String TableName = Prepared.TableName;
            TableDef TableDef = Prepared.TableDef;
            OracleCommand Cmd = Prepared.Cmd;

            List<TableDef_Fields> List_TableDef = new List<TableDef_Fields>();

            switch (SaveDataRow_Process)
            {
                case eSaveDataRow_Process.Process_Insert:
                case eSaveDataRow_Process.Process_Update:
                    {
                        List_TableDef = TableDef.ToList();
                        break;
                    }
                case eSaveDataRow_Process.Process_Delete:
                    {
                        List_TableDef = TableDef.Where(O => O.Is_PK).ToList();
                        break;
                    }
            }

            foreach (var Item_Def in List_TableDef)
            {
                if (Data.Table.Columns.Contains(Item_Def.FieldName))
                {
                    String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);
                    Cmd.Parameters[ParamName].Value = Data[Item_Def.FieldName];
                }
            }

            Cmd.ExecuteNonQuery();

            if (SaveDataRow_Process == eSaveDataRow_Process.Process_Insert)
            {
                //Retrieve Output for PK
                Dictionary<String, Object> PK_Output = new Dictionary<String, Object>();
                foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
                {
                    String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);
                    PK_Output.Add(Item_Def.FieldName, Cmd.Parameters[ParamName].Value);

                    Data[Item_Def.FieldName] = ConvertValue(Item_Def.DbType, Item_Def.Is_Nullable, Cmd.Parameters[ParamName].Value);
                }
            }
        }

        public static void DeleteData(DataTable Data, String TableName, OracleConnection Cn = null)
        {
            //Prevent unwanted table purges with this!
            if (Data.Rows.Count == 0)
            { return; }

            Boolean Is_Cn = Cn != null;

            var Prepared = Prepare_SaveDataRow(eSaveDataRow_Process.Process_Delete, TableName, Data, Cn);

            Int32 RowCt = 0;
            foreach (DataRow Item_DataRow in Data.Rows)
            {
                Execute_SaveDataRow(eSaveDataRow_Process.Process_Delete, Prepared, Item_DataRow);
                RowCt++;
                if (Ev_SaveData_ExecuteSaveDataRow != null)
                { Ev_SaveData_ExecuteSaveDataRow(RowCt, Data.Rows.Count); }
            }

            //If Connection is not supplied, close the connection in the command obj
            if (!Is_Cn)
            { Prepared.Cmd.Connection.Close(); }
        }

        #endregion

        #region _Methods.SaveData<T>

        public static Boolean SaveData<T>(
            eSaveData_Process ProcessType
            , ref T Data
            , String TableName = ""
            , OracleConnection Cn = null
            , EntityKeys Keys = null)
        where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            if (!Check_TableExists(TableName, Cn))
            { throw new Exception($"Oracle Table {TableName} doesn't exist"); }

            IList<T> ListData = new List<T>();
            ListData.Add(Data);

            Boolean SaveData_Result =
                SaveData(
                    ProcessType
                    , ref ListData
                    , TableName
                    , Cn
                    , Keys);

            Data = ListData.First();

            return SaveData_Result;
        }

        public static Boolean SaveData<T>(
            eSaveData_Process ProcessType
            , ref IList<T> Data
            , String TableName = ""
            , OracleConnection Cn = null
            , EntityKeys Keys = null)
        where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            if (!Check_TableExists(TableName, Cn))
            { throw new Exception($"Oracle Table {TableName} doesn't exist"); }

            Boolean Is_Cn = Cn != null;

            eSaveDataRow_Process SaveDataRowProcessType = eSaveDataRow_Process.Process_Insert;
            switch (ProcessType)
            {
                case eSaveData_Process.Process_Insert:
                    SaveDataRowProcessType = eSaveDataRow_Process.Process_Insert;
                    break;
                case eSaveData_Process.Process_Update:
                    SaveDataRowProcessType = eSaveDataRow_Process.Process_Update;
                    break;
            }

            var Prepared =
               Prepare_SaveDataRow<T>(
                   SaveDataRowProcessType
                   , TableName
                   , Cn
                   , Keys);

            Int32 RowCt = 0;
            foreach (var Item_Data in Data)
            {
                Execute_SaveDataRow(SaveDataRowProcessType, Prepared, Item_Data);
                RowCt++;
                if (Ev_SaveData_ExecuteSaveDataRow != null)
                { Ev_SaveData_ExecuteSaveDataRow(RowCt, Data.Count); }
            }

            //If Connection is not supplied, close the connection in the command obj
            if (!Is_Cn)
            { Prepared.Cmd.Connection.Close(); }

            return true;
        }

        static Prepare_SaveDataRow_Returned Prepare_SaveDataRow<T>(
            eSaveDataRow_Process SaveDataRow_Process
            , String TableName
            , OracleConnection Cn = null
            , EntityKeys Keys = null)
        {
            Prepare_SaveDataRow_Returned Returned =
               new Prepare_SaveDataRow_Returned() { Cmd = null, Result = false };

            //Get Table Column Definition
            TableDef TableDef = Get_TableDef(TableName, Cn);

            //If Param Keys is present, replace the keys in TableDef
            if (Keys != null)
            {
                TableDef.ForEach(O =>
                {
                    O.Is_PK = false;
                    var Key = Keys.FirstOrDefault(O_Key => O_Key.Name == O.FieldName);
                    if (Key != null)
                    {
                        O.Is_PK = true;
                        O.Is_Identity = Key.Is_Identity;
                    }
                });
            }

            Returned.TableName = TableName;
            Returned.TableDef = TableDef;

            //List<String> Entity_Fields = typeof(T).GetProperties().Select(O => O.Name).ToList();

            List<String> Entity_Fields = EntityHelper.Get_EntityFields<T>().Select(O => O.FieldName).ToList();

            switch (SaveDataRow_Process)
            {
                case eSaveDataRow_Process.Process_Insert:
                    {
                        Returned = Prepare_SaveDataRow_Insert(Returned, Entity_Fields, Cn);
                        break;
                    }
                case eSaveDataRow_Process.Process_Update:
                    {
                        Returned = Prepare_SaveDataRow_Update(Returned, Entity_Fields, Cn);
                        break;
                    }
                case eSaveDataRow_Process.Process_Delete:
                    {
                        Returned = Prepare_SaveDataRow_Delete(Returned, Cn);
                        break;
                    }
            }

            return Returned;
        }

        static void Execute_SaveDataRow<T>(eSaveDataRow_Process SaveDataRow_Process, Prepare_SaveDataRow_Returned Prepared, T Data)
            where T : class
        {
            //Execute Prepared Query
            String TableName = Prepared.TableName;
            TableDef TableDef = Prepared.TableDef;
            OracleCommand Cmd = Prepared.Cmd;

            List<TableDef_Fields> List_TableDef = new List<TableDef_Fields>();

            switch (SaveDataRow_Process)
            {
                case eSaveDataRow_Process.Process_Insert:
                case eSaveDataRow_Process.Process_Update:
                    {
                        List_TableDef = TableDef.ToList();
                        break;
                    }
                case eSaveDataRow_Process.Process_Delete:
                    {
                        List_TableDef = TableDef.Where(O => O.Is_PK).ToList();
                        break;
                    }
            }

            foreach (var Item_Def in List_TableDef)
            {
                //if (Data.HasProperty(Item_Def.FieldName))
                if (Data.Has_EntityField(Item_Def.FieldName))
                {
                    String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);
                    //Cmd.Parameters[ParamName].Value = Data.GetPropertyValue(Item_Def.FieldName);
                    Cmd.Parameters[ParamName].Value = Data.Get_EntityValue(Item_Def.FieldName);
                }
            }

            Cmd.ExecuteNonQuery();

            if (SaveDataRow_Process == eSaveDataRow_Process.Process_Insert)
            {
                //Retrieve Output for PK
                Dictionary<String, Object> PK_Output = new Dictionary<String, Object>();
                foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
                {
                    String ParamName = String.Format(":P_{0}", Item_Def.FieldCt);
                    PK_Output.Add(Item_Def.FieldName, Cmd.Parameters[ParamName].Value);
                    //Data.SetPropertyValue(Item_Def.FieldName, ConvertValue(Item_Def.DbType, Item_Def.Is_Nullable, Cmd.Parameters[ParamName].Value));
                    Data.Set_EntityValue(Item_Def.FieldName, ConvertValue(Item_Def.DbType, Item_Def.Is_Nullable, Cmd.Parameters[ParamName].Value));
                }
            }
        }

        public static void DeleteData<T>(T Data, String TableName = "", OracleConnection Cn = null) where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            IList<T> ListData = new List<T>();
            ListData.Add(Data);

            DeleteData(ListData, TableName, Cn);
        }

        public static void DeleteData<T>(IList<T> Data, String TableName = "", OracleConnection Cn = null) where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            var Converted_Data = DatabaseMapping.ConvertToDataTable(Data);
            DeleteData(Converted_Data, TableName, Cn);
        }

        #endregion

        #region _Methods.Various

        public static Boolean Check_TableExists(String TableName, OracleConnection Cn = null)
        {
            String Query =
@"
Select Count(1) Ct
From ALL_TABLES
Where TABLE_NAME = :P_TableName
";

            List<OracleParameter> Params = new List<OracleParameter>();
            Params.Add(new OracleParameter(":P_TableName", TableName) { OracleDbType = OracleDbType.Varchar2, Direction = System.Data.ParameterDirection.Input });
            DataTable Dt = OracleDBHelper.ExecuteQuery(Query, Params, Cn);

            if (CommonMethods.Convert_Int32(Dt.Rows[0]["Ct"]) > 0)
            { return true; }
            else
            { return false; }
        }

        public static TableDef Get_TableDef(String TableName, OracleConnection Cn = null)
        {
            String Query =
@"
Select 
    Col.COLUMN_NAME
    , Col.DATA_TYPE
    , Col.DATA_LENGTH
    , Col.DATA_PRECISION
    , Col.DATA_SCALE
    , Col.CHAR_LENGTH
    , (
    Case
        When Col.NULLABLE = 'Y' Then 1
        Else 0
    End
    ) Is_Nullable
    , (
    Case
        When ConCol.COLUMN_NAME Is Not Null Then 1
        Else 0
    End) Is_PK    
From 
    ALL_TAB_COLS Col 
    Left Join ALL_CONSTRAINTS Con
        On Con.TABLE_NAME = Col.TABLE_NAME
        And Con.CONSTRAINT_TYPE = 'P'
    Left Join ALL_CONS_COLUMNS ConCol
        On ConCol.TABLE_NAME = Con.TABLE_NAME
        And ConCol.CONSTRAINT_NAME = Con.CONSTRAINT_NAME
        And ConCol.COLUMN_NAME = Col.COLUMN_NAME
Where 
    Col.TABLE_NAME = :P_TableName
    And Col.VIRTUAL_COLUMN = 'NO'
";

            List<OracleParameter> Params = new List<OracleParameter>();
            Params.Add(new OracleParameter(":P_TableName", TableName) { OracleDbType = OracleDbType.Varchar2, Direction = System.Data.ParameterDirection.Input });
            DataTable Dt = OracleDBHelper.ExecuteQuery(Query, Params, Cn);

            TableDef Def = new TableDef();
            Int32 Ct = 0;
            foreach (DataRow Item_Dr in Dt.Rows)
            {
                Ct++;

                Boolean Is_Nullable = CommonMethods.Convert_Int32(Item_Dr["Is_Nullable"]) == 1 ? true : false;
                Boolean Is_PK = CommonMethods.Convert_Int32(Item_Dr["Is_PK"]) == 1 ? true : false;

                Def.Add(new TableDef_Fields()
                {
                    FieldCt = Ct,
                    FieldName = CommonMethods.Convert_String(Item_Dr["COLUMN_NAME"]),
                    FieldType = ConvertOracleType(CommonMethods.Convert_String(Item_Dr["DATA_TYPE"]), Is_Nullable),
                    DbType = CommonMethods.Convert_String(Item_Dr["DATA_TYPE"]),
                    Is_Nullable = Is_Nullable,
                    Is_PK = Is_PK,
                    Is_Identity = Is_PK,
                    Length = CommonMethods.Convert_Int32(Item_Dr["DATA_LENGTH"]),
                    Precision = CommonMethods.Convert_Int32(Item_Dr["DATA_PRECISION"]),
                    Scale = CommonMethods.Convert_Int32(Item_Dr["DATA_SCALE"])
                });
            }

            return Def;
        }

        public static DataTable CreateDataTable(String TableName, OracleConnection Cn = null)
        {
            /*
            TableDef TableDef = Get_TableDef(TableName);
            return CreateDataTable(TableDef);
            */

            return ExecuteQuery(String.Format("Select * From {0} Where 1 = 0", TableName), (QueryParameters)null, Cn).Clone();
        }

        static DataTable CreateDataTable(TableDef TableDef)
        {
            DataTable Table = new DataTable();
            foreach (var Item_Def in TableDef)
            {
                Type FieldType = Item_Def.FieldType;
                if (Item_Def.FieldType.IsGenericType)
                {
                    if (Item_Def.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    { FieldType = Nullable.GetUnderlyingType(Item_Def.FieldType); }
                }

                DataColumn Dc = new DataColumn(Item_Def.FieldName, FieldType);
                Table.Columns.Add(Dc);
            }

            return Table;
        }

        public static Type ConvertOracleType(String OracleType, Boolean Is_Nullable)
        {
            Type DataType = null;

            switch (OracleType.ToUpper())
            {
                case "VARCHAR2":
                case "NVARCHAR2":
                case "CHAR":
                case "NCHAR":
                    DataType = typeof(String);
                    break;
                case "NUMBER":
                    if (Is_Nullable)
                    { DataType = typeof(Double?); }
                    else
                    { DataType = typeof(Double); }
                    break;
                case "DATE":
                    if (Is_Nullable)
                    { DataType = typeof(DateTime?); }
                    else
                    { DataType = typeof(DateTime); }
                    break;
                case "BLOB":
                    DataType = typeof(Byte[]);
                    break;
            }

            return DataType;
        }

        public static OracleDbType GetOracleType(String OracleType)
        {
            OracleDbType DataType = OracleDbType.NVarchar2;

            switch (OracleType.ToUpper())
            {
                case "VARCHAR2":
                    DataType = OracleDbType.Varchar2;
                    break;
                case "NVARCHAR2":
                    DataType = OracleDbType.NVarchar2;
                    break;
                case "CHAR":
                    DataType = OracleDbType.Char;
                    break;
                case "NCHAR":
                    DataType = OracleDbType.NChar;
                    break;
                case "BLOB":
                    DataType = OracleDbType.Blob;
                    break;
                case "NUMBER":
                    DataType = OracleDbType.Double;
                    break;
                case "DATE":
                    DataType = OracleDbType.Date;
                    break;
            }

            return DataType;
        }

        public static OracleDbType ConvertTypeToOracleType(Type DataType)
        {
            OracleDbType OracleType = OracleDbType.Varchar2;

            if (DataType == typeof(String))
            { OracleType = OracleDbType.Varchar2; }
            else if (DataType == typeof(DateTime))
            { OracleType = OracleDbType.Date; }
            else if (DataType == typeof(Int32))
            { OracleType = OracleDbType.Int32; }
            else if (DataType == typeof(Int64))
            { OracleType = OracleDbType.Long; }
            else if (DataType == typeof(Decimal))
            { OracleType = OracleDbType.Decimal; }
            else if (DataType == typeof(Double))
            { OracleType = OracleDbType.Double; }
            else if (DataType == typeof(Byte[]))
            { OracleType = OracleDbType.Blob; }

            return OracleType;
        }

        public static DbType ConvertTypeToDbType(Type Type)
        {
            var DbType = mDbTypeMap[Type];
            return DbType;
        }

        public static Object ConvertValue(String OracleType, Boolean IsNullable, Object Value)
        {
            Type DataType = ConvertOracleType(OracleType, IsNullable);
            return CommonMethods.Convert_Value(DataType, Value);
        }

        #endregion
    }

    #endregion

    #region _OracleDatabaseConnectionInstance

    public class OracleDatabaseConnectionInstance
    {
        #region _Variables

        String mConnectionString = "";

        #endregion

        #region _Constructor

        public OracleDatabaseConnectionInstance(
            String Host
            , Int32 Port
            , String ServiceID
            , String UserID
            , String Password)
        {
            String ConnectionStringTemplate = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SID={2})));User Id={3};Password={4};";
            String ConnectionString =
                String.Format(
                ConnectionStringTemplate
                , Host
                , Port
                , ServiceID
                , UserID
                , Password);

            this.mConnectionString = ConnectionString;
        }

        #endregion

        #region _Methods.ExecuteQuery

        public DataTable ExecuteQuery(String Query, List<OracleParameter> Params, CommandType Type = CommandType.Text)
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.ExecuteQuery(Query, Params, Type, Cn);
            }
        }

        public DataTable ExecuteQuery(String Query, QueryParameters Params = null)
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.ExecuteQuery(Query, Params, Cn);
            }
        }

        public DataTable ExecuteQuery(OracleCommand Cmd)
        {
            return OracleDBHelper.ExecuteQuery(Cmd);
        }

        public List<T_Entity> ExecuteQuery<T_Entity>(String Query, QueryParameters Params = null) where T_Entity : class, new()
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.ExecuteQuery<T_Entity>(Query, Params, Cn);
            }
        }

        public List<T_Entity> ExecuteQuery<T_Entity>(String Query, List<OracleParameter> Params, CommandType Type = CommandType.Text) where T_Entity : class, new()
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.ExecuteQuery<T_Entity>(Query, Params, Type, Cn);
            }
        }

        public List<T_Entity> ExecuteQuery<T_Entity>(OracleCommand Cmd) where T_Entity : class, new()
        {
            return OracleDBHelper.ExecuteQuery<T_Entity>(Cmd);
        }

        #endregion

        #region _Methods.ExecuteNonQuery

        public Int32 ExecuteNonQuery(String Query, QueryParameters Params = null, CommandType Type = CommandType.Text)
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.ExecuteNonQuery(Query, Params, Cn, Type);
            }
        }

        public Int32 ExecuteNonQuery(String Query, List<OracleParameter> Params, CommandType Type = CommandType.Text)
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.ExecuteNonQuery(Query, Params, Cn, Type);
            }
        }

        #endregion

        #region _Methods.SaveData

        public Boolean SaveData(eSaveData_Process ProcessType, DataTable Data, String TableName = "", EntityKeys Keys = null)
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.SaveData(ProcessType, Data, TableName, Cn, Keys);
            }
        }

        public Boolean SaveData<T_Entity>(eSaveData_Process ProcessType, ref T_Entity Data, String TableName = "", EntityKeys Keys = null) where T_Entity : class, new()
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.SaveData<T_Entity>(ProcessType, ref Data, TableName, Cn, Keys);
            }
        }

        public Boolean SaveData<T_Entity>(eSaveData_Process ProcessType, ref IList<T_Entity> Data, String TableName = "", EntityKeys Keys = null) where T_Entity : class, new()
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                return OracleDBHelper.SaveData<T_Entity>(ProcessType, ref Data, TableName, Cn, Keys);
            }
        }

        public void DeleteData(DataTable Data, String TableName)
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                OracleDBHelper.DeleteData(Data, TableName);
            }
        }

        public void DeleteData<T_Entity>(T_Entity Data, String TableName) where T_Entity : class, new()
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                OracleDBHelper.DeleteData(Data, TableName);
            }
        }

        public void DeleteData<T_Entity>(IList<T_Entity> Data, String TableName) where T_Entity : class, new()
        {
            using (OracleConnection Cn = new OracleConnection(this.mConnectionString))
            {
                Cn.Open();
                OracleDBHelper.DeleteData(Data, TableName, Cn);
            }
        }

        #endregion
    }

    #endregion

    #region _QueryParameters

    [Serializable()]
    public class QueryParameters : List<QueryParameter>
    {
        static QueryParameters()
        {
            //Mapper.Initialize(Cfg => { Cfg.CreateMap<QueryParameters, QueryParameters>(); });
        }

        public String pQuery { get; set; }

        public void Add(String Name, Type Type, Object Value)
        {
            this.Add(Name, Type, Value, ParameterDirection.Input);
        }

        public void Add(String Name, Type Type, Object Value, ParameterDirection Direction)
        {
            base.Add(new QueryParameter() { Name = Name, Type = Type, Value = Value, Direction = Direction });
        }

        public QueryParameters Clone()
        {
            QueryParameters Output = null;
            Output = EntityMapping.MapEntity(this);
            return Output;
        }
    }

    [Serializable()]
    public class QueryParameter
    {
        public String Name { get; set; }
        public Object Value { get; set; }
        public Type Type { get; set; }
        public ParameterDirection Direction { get; set; }
    }

    #endregion
}
