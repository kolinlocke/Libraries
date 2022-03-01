using Commons;
using Commons.EntityProps;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.SQLServer.Common
{
    #region _SQLServerManager

    public class SQLServerManager
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
            public SqlCommand Cmd;
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

        public String mConnectionString;
        public SqlConnection mCn;

        #endregion

        #region _Properties

        /// <summary>
        /// Get or set the connection string to be used by query builder
        /// </summary>
        public static string ConnectionString { get; set; }

        #endregion

        #region _Constructor

        static SQLServerManager()
        {
            Setup_DbTypeMap();
        }

        public SQLServerManager(
            String Server
            , Int32 Port
            , String Database
            , String UserID
            , String Password)
        {
            String ConnectionString = $"Server={Server},{Port};Database={Database};User Id={UserID};Password={Password};";
            this.mConnectionString = ConnectionString;
        }

        public SQLServerManager(
            String Server
            , String Database
            , String UserID
            , String Password)
        {
            String ConnectionString = $"Server={Server};Database={Database};User Id={UserID};Password={Password};";
            this.mConnectionString = ConnectionString;
            //this.mCn = new SqlConnection(this.mConnectionString);
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

        #region _Methods.ExecuteQuery

        public DataTable ExecuteQuery(String Query, QueryParameters Params, SqlConnection Cn = null)
        {
            List<SqlParameter> SqlParams =
                (Params ?? new QueryParameters()).Select(O =>
                    new SqlParameter()
                    {
                        ParameterName = O.Name,
                        Value = O.Value,
                    }).ToList();

            return ExecuteQuery(Query, SqlParams, CommandType.Text, Cn);
        }

        public DataTable ExecuteQuery(String Query, List<SqlParameter> Params, CommandType Type = CommandType.Text, SqlConnection Cn = null)
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new SqlConnection(ConnectionString);
                Cn.Open();
            }

            SqlCommand Cmd = new SqlCommand();
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Cmd.CommandType = Type;

            if (Params != null)
            { Params.ForEach(O => Cmd.Parameters.Add(O)); }

            DataSet Ds_QueryResult = new DataSet();
            SqlDataAdapter Adp = new SqlDataAdapter(Cmd);
            Adp.Fill(Ds_QueryResult);

            if (Is_Cn)
            { Cn.Close(); }

            return Ds_QueryResult.Tables[0];
        }

        public DataTable ExecuteQuery(SqlCommand Cmd)
        {
            DataSet Ds_QueryResult = new DataSet();
            SqlDataAdapter Adp = new SqlDataAdapter(Cmd);
            Adp.Fill(Ds_QueryResult);

            return Ds_QueryResult.Tables[0];
        }

        #endregion

        #region _Methods.ExecuteQuery<T_Entity>

        public List<T_Entity> ExecuteQuery<T_Entity>(String Query, QueryParameters Params, SqlConnection Cn = null) where T_Entity : class, new()
        {


            return null;
        }

        public List<T_Entity> ExecuteQuery<T_Entity>(
            String Query
            , List<SqlParameter> Params
            , CommandType Type = CommandType.Text
            , SqlConnection Cn = null)
        where T_Entity : class, new()
        {


            return null;
        }

        public static List<T> ExecuteQuery<T>(SqlCommand Cmd) where T : class, new()
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
