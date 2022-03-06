using Commons;
using Commons.EntityProps;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
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
            public SqlDbType SqlDbType { get; set; }
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

        internal readonly Dictionary<SqlDbType, Type> equivalentSystemType = new Dictionary<SqlDbType, Type>
        {
            { SqlDbType.BigInt, typeof(long)},
            { SqlDbType.Binary, typeof(byte[]) },
            { SqlDbType.Bit, typeof(bool) },
            { SqlDbType.Char, typeof(string) },
            { SqlDbType.Date, typeof(DateTime) },
            { SqlDbType.DateTime, typeof(DateTime) },
            { SqlDbType.DateTime2, typeof(DateTime) }, // SQL2008+
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset) }, // SQL2008+
            { SqlDbType.Decimal, typeof(decimal) },
            { SqlDbType.Float, typeof(double) },
            { SqlDbType.Image, typeof(byte[]) },
            { SqlDbType.Int, typeof(int) },
            { SqlDbType.Money, typeof(decimal) },
            { SqlDbType.NChar, typeof(string) },
            { SqlDbType.NVarChar, typeof(string) },
            { SqlDbType.Real, typeof(float) },
            { SqlDbType.SmallDateTime, typeof(DateTime) },
            { SqlDbType.SmallInt, typeof(short) },
            { SqlDbType.SmallMoney, typeof(decimal) },
            { SqlDbType.Time, typeof(TimeSpan) }, // SQL2008+
            { SqlDbType.TinyInt, typeof(byte) },
            { SqlDbType.UniqueIdentifier, typeof(Guid) },
            { SqlDbType.VarBinary, typeof(byte[]) },
            { SqlDbType.VarChar, typeof(string) },
            { SqlDbType.Xml, typeof(SqlXml) }
            // omitted special types: timestamp
            // omitted deprecated types: ntext, text
            // not supported by enum: numeric, FILESTREAM, rowversion, sql_variant
        };

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

        #region _Methods.PrepareQuery

        public SqlCommand PrepareQuery(
            String Query
            , QueryParameters Params
            , SqlConnection Cn = null
            , CommandType Type = CommandType.Text)
        {
            List<SqlParameter> OracleParams =
               Params.Select(O =>
                   new SqlParameter()
                   {
                       ParameterName = O.Name,
                       Value = O.Value,
                       SqlDbType = OracleDBHelper.ConvertTypeToOracleType(O.Type)
                   }).ToList();

            return PrepareQuery(Query, OracleParams, Cn, Type);
        }

        public SqlCommand PrepareQuery(
            String Query
            , List<SqlParameter> Params = null
            , SqlConnection Cn = null
            , CommandType Type = CommandType.Text)
        {
            if (Cn == null)
            {
                Cn = new SqlConnection(ConnectionString);
                Cn.Open();
            }

            SqlCommand Cmd = new SqlCommand();
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Params.ForEach(O => Cmd.Parameters.Add(O));
            Cmd.CommandType = Type;
            Cmd.Prepare();

            return Cmd;
        }

        #endregion

        #region _Methods.ExecuteQuery

        public DataTable ExecuteQuery(
            String Query
            , QueryParameters Params
            , CommandType Type = CommandType.Text
            , SqlConnection Cn = null)
        {
            List<SqlParameter> SqlParams =
                (Params ?? new QueryParameters()).Select(O =>
                    new SqlParameter()
                    {
                        ParameterName = O.Name,
                        Value = O.Value,
                    }).ToList();

            return ExecuteQuery(Query, SqlParams, Type, Cn);
        }

        public DataTable ExecuteQuery(
            String Query
            , List<SqlParameter> Params
            , CommandType Type = CommandType.Text
            , SqlConnection Cn = null)
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

        public List<T_Entity> ExecuteQuery<T_Entity>(
            String Query
            , QueryParameters Params
            , CommandType Type = CommandType.Text
            , SqlConnection Cn = null)
        where T_Entity : class, new()
        {
            List<SqlParameter> SqlParams =
               (Params ?? new QueryParameters()).Select(O =>
                   new SqlParameter()
                   {
                       ParameterName = O.Name,
                       Value = O.Value,
                   }).ToList();

            return ExecuteQuery<T_Entity>(Query, SqlParams, Type, Cn);
        }

        public List<T_Entity> ExecuteQuery<T_Entity>(
            String Query
            , List<SqlParameter> Params
            , CommandType Type = CommandType.Text
            , SqlConnection Cn = null)
        where T_Entity : class, new()
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

            List<T_Entity> Returned = ExecuteQuery<T_Entity>(Cmd);

            if (Is_Cn)
            { Cn.Close(); }

            return Returned;
        }

        public List<T> ExecuteQuery<T>(SqlCommand Cmd) where T : class, new()
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

        #region _Methods.ExecuteNonQuery

        public Int32 ExecuteNonQuery(
            String Query
            , QueryParameters Params
            , CommandType Type = CommandType.Text
            , SqlConnection Cn = null)
        {
            List<SqlParameter> SqlParams =
               (Params ?? new QueryParameters()).Select(O =>
                   new SqlParameter()
                   {
                       ParameterName = O.Name,
                       Value = O.Value,
                       Direction = O.Direction
                   }).ToList();

            var Returned = ExecuteNonQuery(Query, SqlParams, Type, Cn);

            SqlParams.ForEach(O_Op =>
            {
                Params.FirstOrDefault(O => O.Name == O_Op.ParameterName).Value = O_Op.Value;
            });

            return Returned;
        }

        public Int32 ExecuteNonQuery(
            String Query
            , List<SqlParameter> Params = null
            , CommandType Type = CommandType.Text
            , SqlConnection Cn = null)
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
            Params.ForEach(O => Cmd.Parameters.Add(O));

            var ExecuteResult = Cmd.ExecuteNonQuery();

            if (Is_Cn)
            { Cn.Close(); }

            return ExecuteResult;
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


        public Boolean SaveData(
          eSaveData_Process ProcessType
          , DataTable Data
          , String TableName = ""
          , SqlConnection Cn = null
          , EntityKeys Keys = null)
        {
            if (!Check_TableExists(TableName, Cn))
            { throw new Exception($"SQL Table {TableName} doesn't exist"); }

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

        Prepare_SaveDataRow_Returned Prepare_SaveDataRow(
            eSaveDataRow_Process SaveDataRow_Process
            , String TableName
            , DataTable Data
            , SqlConnection Cn = null
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

        Prepare_SaveDataRow_Returned Prepare_SaveDataRow_Insert(
            Prepare_SaveDataRow_Returned Returned
            , List<String> Entity_Fields
            , SqlConnection Cn = null)
        {
            String TableName = Returned.TableName;
            TableDef TableDef = Returned.TableDef;

            List<SqlParameter> Params = new List<SqlParameter>();
            List<String> Query_Insert_Fields = new List<String>();
            List<String> Query_Insert_Field_Params = new List<String>();

            //Add all non PK fields to Params
            foreach (var Item_Def in TableDef.ToList().Where(O => (!O.Is_PK || (O.Is_PK && !O.Is_Identity)) && Entity_Fields.Contains(O.FieldName)))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = String.Format("@P_{0}", Item_Def.FieldCt);

                Query_Insert_Fields.Add(Item_Def.FieldName);
                Query_Insert_Field_Params.Add(ParamName);

                SqlParameter Param = new SqlParameter();
                Param.ParameterName = ParamName;
                Param.SqlDbType = Item_Def.SqlDbType;
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            List<String> Query_Returning_Fields = new List<String>();
            List<String> Query_Returning_Params = new List<String>();

            //Process PK Fields for output
            foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK && O.Is_Identity))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = String.Format("@P_{0}", Item_Def.FieldCt);

                Query_Returning_Fields.Add(Item_Def.FieldName);
                Query_Returning_Params.Add(ParamName);

                SqlParameter Param = new SqlParameter();
                Param.ParameterName = ParamName;
                Param.SqlDbType = Item_Def.SqlDbType;
                Param.Direction = ParameterDirection.Output;
                Params.Add(Param);
            }

            //TO DO:
            //Recode this part for SQL Server Syntax

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

            Returned.Cmd = this.PrepareQuery(Query, Params, Cn);
            Returned.Insert_Params.Query_Insert_Params = Query_Insert_Field_Params;
            Returned.Insert_Params.Query_Insert_Returning_Params = Query_Returning_Params;

            return Returned;
        }


        #endregion

        #region _Methods.Various

        public Boolean Check_TableExists(String TableName, SqlConnection Cn = null)
        {
            String Query =
@"
Select Count(1) Ct
From Sys.Tables
Where Name = @P_TableName
";

            List<SqlParameter> Params = new List<SqlParameter>();
            Params.Add(new SqlParameter("@P_TableName", TableName) { SqlDbType = SqlDbType.VarChar, Direction = System.Data.ParameterDirection.Input });
            DataTable Dt = this.ExecuteQuery(Query, Params, Cn: Cn);

            if (CommonMethods.Convert_Int32(Dt.Rows[0]["Ct"]) > 0)
            { return true; }
            else
            { return false; }
        }

        public TableDef Get_TableDef(String TableName, SqlConnection Cn = null)
        {
            String Query =
@"
Select
	sCol.Column_id
	, sCol.Name As [ColumnName]
	, sTyp.Name As [DataType]
	, sCol.max_length As [Length]
	, sCol.Precision
	, sCol.Scale
	, sCol.Is_Identity As [Is_Identity]
    , sCol.Is_Nullable As [Is_Nullable]
	, Cast
	(
		(
		Case Count(IsCcu.Column_Name)
			When 0 Then 0
			Else 1
		End
		) 
	As Bit) As IsPk
From 
	Sys.Columns As sCol
	Left Join Sys.Types As sTyp
		On sCol.system_type_id = sTyp.system_type_id
	Inner Join Sys.Tables As sTab
		On sCol.Object_ID = sTab.Object_ID
	Left Join Sys.Key_Constraints As Skc
		On sTab.Object_Id = Skc.Parent_Object_Id
		And Skc.Type = 'PK'		
	Left Join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE As IsCcu
		On Skc.Name = IsCcu.Constraint_Name
		And sTab.Name = IsCcu.Table_Name
		And sCol.Name = IsCcu.Column_Name
Where
	sTab.Name = @TableName
	And sCol.Is_Computed = 0
Group By
	sCol.Name
	, sTyp.Name
	, sCol.max_length
	, sCol.Precision
	, sCol.Scale
	, sCol.Is_Identity
    , sCol.Is_Nullable
	, sCol.Column_id
";

            List<SqlParameter> Params = new List<SqlParameter>();
            Params.Add(new SqlParameter("@P_TableName", TableName) { SqlDbType = SqlDbType.VarChar, Direction = System.Data.ParameterDirection.Input });
            DataTable Dt = this.ExecuteQuery(Query, Params, CommandType.Text, Cn);

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
                    FieldName = CommonMethods.Convert_String(Item_Dr["ColumnName"]),
                    FieldType = ConvertSqlType(CommonMethods.Convert_String(Item_Dr["DataType"]), Is_Nullable),
                    SqlDbType = CommonMethods.ParseEnum<SqlDbType>(CommonMethods.Convert_String(Item_Dr["DataType"])),
                    Is_Nullable = Is_Nullable,
                    Is_PK = Is_PK,
                    Is_Identity = Is_PK,
                    Length = CommonMethods.Convert_Int32(Item_Dr["Length"]),
                    Precision = CommonMethods.Convert_Int32(Item_Dr["Precision"]),
                    Scale = CommonMethods.Convert_Int32(Item_Dr["Scale"])
                });
            }

            return Def;
        }

        public Type ConvertSqlType(String SqlType, Boolean Is_Nullable)
        {
            var SqlDbType = CommonMethods.ParseEnum<SqlDbType>(SqlType);
            var DataType = this.equivalentSystemType[SqlDbType];

            return DataType;
        }

        public SqlDbType GetSqlDbType(Type DataType)
        {
            var Ret_SqlDbType = this.equivalentSystemType.FirstOrDefault(O => O.Value == DataType).Key;

            return Ret_SqlDbType;
        }

        #endregion
    }

    #endregion

    #region _QueryParameters

    [Serializable()]
    public class QueryParameters : List<QueryParameter>
    {
        static QueryParameters() { }

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
