using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using Commons;
using System.Data.SqlTypes;
using Commons.EntityProps;

namespace Data.Sqlite.Manager
{
    #region _Class.SqliteManager

    public class SqliteManager
    {
        #region _Definitions

        public class TableDef : List<TableDef_Fields> { }

        public class TableDef_Fields
        {
            public TableDef_Fields()
            {
                FieldName = "";
                FieldType = typeof(String);
            }

            public String FieldName { get; set; }
            public Type FieldType { get; set; }
            public Int32 Length { get; set; }
            public Int32 Precision { get; set; }
            public Int32 Scale { get; set; }
            public Boolean Is_Nullable { get; set; }
            public Boolean Is_PK { get; set; }
            public Boolean Is_Identity { get; set; }
            //public DbType DbType { get; set; }
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
            public SQLiteCommand? Cmd;
            public Boolean Result;
            public String TableName;
            public TableDef TableDef;
            public Prepare_SaveDataRow_Returned_Insert Insert_Params;
            public Prepare_SaveDataRow_Returned_Update Update_Params;
            public Prepare_SaveDataRow_Returned_Delete Delete_Params;
            public Boolean Insert_HasOutput;
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
            public String? Name { get; set; }
            public Object? Value { get; set; }
            public Type? Type { get; set; }
            public Boolean Is_Identity { get; set; }
        }

        readonly Dictionary<Type, String> SQLiteTypeMap = new Dictionary<Type, String>()
        {
            {typeof(Int64), "INTEGER"},
            {typeof(Int32), "INTEGER"},
            {typeof(Int16), "INTEGER"},
            {typeof(Boolean), "INTEGER"},
            {typeof(String), "TEXT"},
            {typeof(DateTime), "TEXT"},
            {typeof(Guid), "TEXT"},
            {typeof(Double), "REAL"},
            {typeof(Decimal), "REAL"},
            {typeof(Single), "REAL"}
        };

        readonly Dictionary<String, DbType> SQLiteDbTypeMap = new Dictionary<String, DbType>()
        {
            {"INTEGER" , DbType.Int64 },
            {"TEXT" , DbType.String },
            {"NUMERIC", DbType.Double },
            {"REAL", DbType.Double }

        };

        readonly Dictionary<DbType, Type> DbTypeMap = new Dictionary<DbType, Type>()
        {
            {DbType.Int64 , typeof(Int64) },
            {DbType.Int32, typeof(Int32) },
            {DbType.Int16 , typeof(Int16) },
            {DbType.Boolean, typeof(Boolean) },
            {DbType.String, typeof(String) },
            {DbType.DateTime , typeof(DateTime) },
            {DbType.Guid , typeof(Guid) },
            {DbType.Double, typeof(Double) },
            {DbType.Decimal, typeof(Decimal) },
            {DbType.Single , typeof(Single) }
        };

        #endregion

        #region _Members

        String mConnectionString = "";
        Boolean mIsConnected = false;

        #endregion

        #region _Constructor

        public SqliteManager() { this.mIsConnected = false; }

        public SqliteManager(String DbFile, String Password)
        {
            //String ConnectionString = $@"Data Source = {DbFile}; Version = 3; Password = {Password}";
            String ConnectionString = $@"Data Source = {DbFile}; Version = 3";
            this.mConnectionString = ConnectionString;
            this.mIsConnected = true;
        }

        #endregion

        #region _Methods.PrepareQuery

        public SQLiteCommand PrepareQuery(
            String Query
            , QueryParameters Params
            , SQLiteConnection? Cn = null
            , CommandType Type = CommandType.Text)
        {
            List<SQLiteParameter> SqlParams =
               Params.Select(O =>
                   new SQLiteParameter()
                   {
                       ParameterName = O.Name,
                       Value = O.Value,
                       DbType = GetDbType(O.Type)
                   }).ToList();

            return PrepareQuery(Query, SqlParams, Cn, Type);
        }

        public SQLiteCommand PrepareQuery(
            String Query
            , List<SQLiteParameter>? Params = null
            , SQLiteConnection? Cn = null
            , CommandType Type = CommandType.Text)
        {
            if (Cn == null)
            {
                Cn = new SQLiteConnection(mConnectionString);
                Cn.Open();
            }

            SQLiteCommand Cmd = new SQLiteCommand(Query);
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            if (Params != null)
            { Params.ForEach(O => Cmd.Parameters.Add(O)); }
            Cmd.CommandType = Type;
            Cmd.Prepare();

            return Cmd;
        }

        #endregion

        #region _Methods.ExecuteQuery

        public DataTable ExecuteQuery(
            String Query
            , List<SQLiteParameter> Params
            , CommandType Type = CommandType.Text
            , SQLiteConnection? Cn = null)
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new SQLiteConnection(mConnectionString);
                Cn.Open();
            }

            SQLiteCommand Cmd = new SQLiteCommand();
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Cmd.CommandType = Type;

            if (Params != null)
            { Params.ForEach(O => Cmd.Parameters.Add(O)); }

            DataSet Ds_QueryResult = new DataSet();
            SQLiteDataAdapter Adp = new SQLiteDataAdapter(Cmd);
            Adp.Fill(Ds_QueryResult);

            if (Is_Cn)
            { Cn.Close(); }

            return Ds_QueryResult.Tables[0];
        }

        public DataTable ExecuteQuery(
            String Query
            , QueryParameters Params
            , CommandType Type = CommandType.Text
            , SQLiteConnection? Cn = null)
        {
            List<SQLiteParameter> SqlParams =
                (Params ?? new QueryParameters()).Select(O =>
                    new SQLiteParameter()
                    {
                        ParameterName = O.Name,
                        Value = O.Value,
                    }).ToList();

            return ExecuteQuery(Query, SqlParams, Type, Cn);
        }

        public DataTable ExecuteQuery(SQLiteCommand Cmd)
        {
            DataSet Ds_QueryResult = new DataSet();
            SQLiteDataAdapter Adp = new SQLiteDataAdapter(Cmd);
            Adp.Fill(Ds_QueryResult);

            return Ds_QueryResult.Tables[0];
        }

        #endregion

        #region _Methods.ExecuteQuery<T_Entity>

        public List<T_Entity> ExecuteQuery<T_Entity>(
            String Query
            , QueryParameters? Params = null
            , CommandType Type = CommandType.Text
            , SQLiteConnection? Cn = null)
        where T_Entity : class, new()
        {
            List<SQLiteParameter> SqlParams =
               (Params ?? new QueryParameters()).Select(O =>
                   new SQLiteParameter()
                   {
                       ParameterName = O.Name,
                       Value = O.Value,
                   }).ToList();

            return ExecuteQuery<T_Entity>(Query, SqlParams, Type, Cn);
        }

        public List<T_Entity> ExecuteQuery<T_Entity>(
            String Query
            , List<SQLiteParameter> Params
            , CommandType Type = CommandType.Text
            , SQLiteConnection? Cn = null)
        where T_Entity : class, new()
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new SQLiteConnection(mConnectionString);
                Cn.Open();
            }

            SQLiteCommand Cmd = new SQLiteCommand();
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

        public List<T> ExecuteQuery<T>(SQLiteCommand Cmd) where T : class, new()
        {
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
            , SQLiteConnection? Cn = null)
        {
            List<SQLiteParameter> SqlParams =
               (Params ?? new QueryParameters()).Select(O =>
                   new SQLiteParameter()
                   {
                       ParameterName = O.Name,
                       Value = O.Value,
                       Direction = O.Direction
                   }).ToList();

            var Returned = ExecuteNonQuery(Query, SqlParams, Type, Cn);

            if (Params != null)
            {
                SqlParams.ForEach(O_Op =>
                {
                    // Params.FirstOrDefault(O => O.Name == O_Op.ParameterName).Value = O_Op.Value;
                    var O_Param = Params.FirstOrDefault(O => O.Name == O_Op.ParameterName);
                    if (O_Param != null)
                    { O_Param.Value = O_Op.Value; }
                });
            }

            return Returned;
        }

        public Int32 ExecuteNonQuery(
            String Query
            , List<SQLiteParameter>? Params = null
            , CommandType Type = CommandType.Text
            , SQLiteConnection? Cn = null)
        {
            Boolean Is_Cn = false;
            if (Cn == null)
            {
                Is_Cn = true;
                Cn = new SQLiteConnection(mConnectionString);
                Cn.Open();
            }

            SQLiteCommand Cmd = new SQLiteCommand();
            Cmd.Connection = Cn;
            Cmd.CommandText = Query;
            Cmd.CommandType = Type;
            if (Params != null)
            { Params.ForEach(O => Cmd.Parameters.Add(O)); }

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
          , SQLiteConnection? Cn = null
          , EntityKeys? Keys = null)
        {
            if (!Check_TableExists(TableName, Cn))
            { throw new Exception($"Database Table {TableName} doesn't exist"); }

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
            if (!Is_Cn && Prepared.Cmd != null)
            { Prepared.Cmd.Connection.Close(); }

            return true;
        }

        Prepare_SaveDataRow_Returned Prepare_SaveDataRow(
            eSaveDataRow_Process SaveDataRow_Process
            , String TableName
            , DataTable Data
            , SQLiteConnection? Cn = null
            , EntityKeys? Keys = null)
        {
            Prepare_SaveDataRow_Returned Returned =
                new Prepare_SaveDataRow_Returned()
                {
                    Insert_HasOutput = false,
                    Cmd = new SQLiteCommand(),
                    Result = false
                };

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
            , SQLiteConnection? Cn = null)
        {
            String TableName = Returned.TableName;
            TableDef TableDef = Returned.TableDef;

            List<SQLiteParameter> Params = new List<SQLiteParameter>();
            List<String> Query_Insert_Fields = new List<String>();
            List<String> Query_Insert_Field_Params = new List<String>();

            //Add all non PK fields to Params
            foreach (var Item_Def in TableDef.ToList().Where(O => (!O.Is_PK || (O.Is_PK && !O.Is_Identity)) && Entity_Fields.Contains(O.FieldName)))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = $"$P_{Item_Def.FieldCt}"; //String.Format("@P_{0}", Item_Def.FieldCt);

                Query_Insert_Fields.Add(Item_Def.FieldName);
                Query_Insert_Field_Params.Add(ParamName);

                SQLiteParameter Param = new SQLiteParameter();
                Param.ParameterName = ParamName;
                Param.Size = Item_Def.Length;
                //Param.DbType = Item_Def.DbType;
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            List<String> Query_Returning_Fields = new List<String>();
            List<String> Query_Returning_Params = new List<String>();

            //Process PK Fields for output
            StringBuilder Query_Output_Fields = new StringBuilder();
            String Query_Output_Fields_Comma = "";

            foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK && O.Is_Identity))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                //String ParamName = $"@P_{Item_Def.FieldCt}"; //String.Format("@P_{0}", Item_Def.FieldCt);

                //Query_Returning_Fields.Add(Item_Def.FieldName);
                //Query_Returning_Params.Add(ParamName);

                //SqlParameter Param = new SqlParameter();
                //Param.ParameterName = ParamName;
                //Param.Size = Item_Def.Length;
                //Param.SqlDbType = Item_Def.SqlDbType;
                //Param.Direction = ParameterDirection.Output;
                //Params.Add(Param);

                Query_Returning_Fields.Add(Item_Def.FieldName);
                Query_Output_Fields.Append($@" {Query_Output_Fields_Comma} ""{Item_Def.FieldName}"" ");

                Query_Output_Fields_Comma = ",";
            }

            //TO DO:
            //Recode this part for SQL Server Syntax

            //String Query_Output_DeclareTable = "";
            //String Query_Output_SelectTable = "";
            String Query_Returning = "";
            if (Query_Returning_Fields.Any())
            {
                Returned.Insert_HasOutput = true;
                Query_Returning = $@"Returning {Query_Output_Fields.ToString()}";
            }

            String Query =
$@"
Insert Into [{TableName}]
    ({String.Join(" , ", Query_Insert_Fields)})
Values
    ({String.Join(" , ", Query_Insert_Field_Params)})
{Query_Returning}    
";


            /*
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
            */

            Returned.Cmd = this.PrepareQuery(Query, Params, Cn);
            Returned.Insert_Params.Query_Insert_Params = Query_Insert_Field_Params;
            //Returned.Insert_Params.Query_Insert_Returning_Params = Query_Returning_Params;

            return Returned;
        }

        Prepare_SaveDataRow_Returned Prepare_SaveDataRow_Update(
          Prepare_SaveDataRow_Returned Returned
          , List<String> Entity_Fields
          , SQLiteConnection? Cn = null)
        {
            String TableName = Returned.TableName;
            TableDef TableDef = Returned.TableDef;

            List<SQLiteParameter> Params = new List<SQLiteParameter>();
            List<String> Query_Update_Fields = new List<String>();
            List<String> Query_Update_Field_Params = new List<String>();
            List<String> Query_Update_Where = new List<String>();
            List<String> Query_Update_Where_Params = new List<String>();

            //Add all non PK fields to Params
            foreach (var Item_Def in TableDef.ToList().Where(O => !O.Is_PK && Entity_Fields.Contains(O.FieldName)))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                //String ParamName = String.Format("@P_{0}", Item_Def.FieldCt);
                String ParamName = $"$P_{Item_Def.FieldCt}";

                String Query_Update_Field = $" {Item_Def.FieldName} = {ParamName} "; //String.Format(" {0} = {1} ", Item_Def.FieldName, ParamName);

                Query_Update_Fields.Add(Query_Update_Field);
                Query_Update_Field_Params.Add(ParamName);

                SQLiteParameter Param = new SQLiteParameter();
                Param.ParameterName = ParamName;
                Param.Size = Item_Def.Length;
                //Param.DbType = Item_Def.DbType;
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            //Process PK Fields for Where Clause
            foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = $"$P_{Item_Def.FieldCt}"; //String.Format("@P_{0}", Item_Def.FieldCt);
                String Query_Where = $" {Item_Def.FieldName} = {ParamName} "; //String.Format(" {0} = {1} ", Item_Def.FieldName, ParamName);

                Query_Update_Where.Add(Query_Where);
                Query_Update_Where_Params.Add(ParamName);

                //OracleParameter Param = new OracleParameter();
                SQLiteParameter Param = new SQLiteParameter();
                Param.ParameterName = ParamName;
                Param.Size = Item_Def.Length;
                //Param.DbType = Item_Def.DbType;
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            String Query =
$@"
Update [{TableName}]
Set
{String.Join(" , ", Query_Update_Fields)}
Where
{String.Join(" And ", Query_Update_Where)}
";

            Returned.Cmd = PrepareQuery(Query, Params, Cn);
            Returned.Update_Params.Query_Update_Params = Query_Update_Field_Params;
            Returned.Update_Params.Query_Update_Where_Params = Query_Update_Where_Params;

            return Returned;
        }

        Prepare_SaveDataRow_Returned Prepare_SaveDataRow_Delete(Prepare_SaveDataRow_Returned Returned, SQLiteConnection? Cn = null)
        {
            String TableName = Returned.TableName;
            TableDef TableDef = Returned.TableDef;

            List<SQLiteParameter> Params = new List<SQLiteParameter>();
            List<String> Query_Update_Where = new List<String>();
            List<String> Query_Delete_Where_Params = new List<String>();

            //Process PK Fields for Where Clause
            foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
            {
                //String ParamName = String.Format(":P_{0}", Item_Def.FieldName.Replace(" ", "_"));
                String ParamName = $"$P_{Item_Def.FieldCt}"; //String.Format(":P_{0}", Item_Def.FieldCt);
                String Query_Where = $" {Item_Def.FieldName} = {ParamName} "; //String.Format(" {0} = {1} ", Item_Def.FieldName, ParamName);

                Query_Update_Where.Add(Query_Where);
                Query_Delete_Where_Params.Add(ParamName);

                SQLiteParameter Param = new SQLiteParameter();
                Param.ParameterName = ParamName;
                Param.Size = Item_Def.Length;
                //Param.DbType = Item_Def.DbType;
                Param.Direction = ParameterDirection.Input;
                Params.Add(Param);
            }

            String Query =
$@"
Delete From [{TableName}]
Where {String.Join(" And ", Query_Update_Where)}
";

            Returned.Cmd = PrepareQuery(Query, Params, Cn);
            Returned.Delete_Params.Query_Delete_Where_Params = Query_Delete_Where_Params;

            return Returned;
        }

        void Execute_SaveDataRow(eSaveDataRow_Process SaveDataRow_Process, Prepare_SaveDataRow_Returned Prepared, DataRow Data)
        {
            //Execute Prepared Query
            String TableName = Prepared.TableName;
            TableDef TableDef = Prepared.TableDef;
            SQLiteCommand? Cmd = Prepared.Cmd;

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

            if (Cmd == null)
            { throw new Exception("Line 788: Cmd is not initialized."); }

            var ParamList = Cmd.Parameters.Cast<SQLiteParameter>().ToList();

            foreach (var Item_Def in List_TableDef)
            {
                if (Data.Table.Columns.Contains(Item_Def.FieldName))
                {
                    String ParamName = $"$P_{Item_Def.FieldCt}"; //String.Format("@P_{0}", Item_Def.FieldCt);
                    if (ParamList.Exists(O => O.ParameterName == ParamName))
                    {
                        if (Item_Def.Is_Nullable)
                        {
                            Cmd.Parameters[ParamName].Value = CommonMethods.IsNull(Data[Item_Def.FieldName], DBNull.Value);
                        }
                        else
                        {
                            Cmd.Parameters[ParamName].Value = CommonMethods.Convert_Value(Item_Def.FieldType, Data[Item_Def.FieldName]);
                        }
                    }
                }
            }

            if (SaveDataRow_Process == eSaveDataRow_Process.Process_Insert)
            {
                //Retrieve Output for PK
                if (Prepared.Insert_HasOutput)
                {
                    DataSet Ds_Output = new DataSet();
                    SQLiteDataAdapter Adp = new SQLiteDataAdapter();
                    Adp.SelectCommand = Cmd;
                    Adp.Fill(Ds_Output);

                    foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
                    {
                        Data[Item_Def.FieldName] = ConvertValue(Item_Def.FieldType, Item_Def.Is_Nullable, Ds_Output.Tables[0].Rows[0][Item_Def.FieldName]);
                    }
                }
                else
                { Cmd.ExecuteNonQuery(); }
            }
            else
            { Cmd.ExecuteNonQuery(); }
        }

        public void DeleteData(DataTable Data, String TableName, SQLiteConnection? Cn = null)
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
            if (!Is_Cn && Prepared.Cmd != null)
            { Prepared.Cmd.Connection.Close(); }
        }

        #endregion

        #region _Methods.SaveData<T>

        public Boolean SaveData<T>(
            eSaveData_Process ProcessType
            , ref IList<T> Data
            , String TableName = ""
            , SQLiteConnection? Cn = null
            , EntityKeys? Keys = null)
        where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            if (!Check_TableExists(TableName, Cn))
            { throw new Exception($"Database Table {TableName} doesn't exist"); }

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
            if (!Is_Cn && Prepared.Cmd != null)
            { Prepared.Cmd.Connection.Close(); }

            return true;
        }

        public Boolean SaveData<T>(
          eSaveData_Process ProcessType
          , ref T Data
          , String TableName = ""
          , SQLiteConnection? Cn = null
          , EntityKeys? Keys = null)
        where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            if (!Check_TableExists(TableName, Cn))
            { throw new Exception($"Database Table {TableName} doesn't exist"); }

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

        Prepare_SaveDataRow_Returned Prepare_SaveDataRow<T>(
            eSaveDataRow_Process SaveDataRow_Process
            , String TableName
            , SQLiteConnection? Cn = null
            , EntityKeys? Keys = null)
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

        void Execute_SaveDataRow<T>(eSaveDataRow_Process SaveDataRow_Process, Prepare_SaveDataRow_Returned Prepared, T Data)
            where T : class
        {
            //Execute Prepared Query
            String TableName = Prepared.TableName;
            TableDef TableDef = Prepared.TableDef;
            SQLiteCommand? Cmd = Prepared.Cmd;

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

            if (Cmd == null)
            { throw new Exception("Line 1023: Cmd is not initialized."); }

            var ParamList = Cmd.Parameters.Cast<SQLiteParameter>().ToList();

            foreach (var Item_Def in List_TableDef)
            {
                //if (Data.HasProperty(Item_Def.FieldName))
                if (Data.Has_EntityField(Item_Def.FieldName))
                {
                    String ParamName = $"$P_{Item_Def.FieldCt}";  //String.Format("@P_{0}", Item_Def.FieldCt);
                    if (ParamList.Exists(O => O.ParameterName == ParamName))
                    {
                        if (Item_Def.Is_Nullable)
                        {
                            Cmd.Parameters[ParamName].Value = CommonMethods.IsNull(Data.Get_EntityValue(Item_Def.FieldName), DBNull.Value);
                        }
                        else
                        {
                            //Cmd.Parameters[ParamName].Value = CommonMethods.IsNull(Data.Get_EntityValue(Item_Def.FieldName), CommonMethods.GetDefault(Item_Def.FieldType));
                            Cmd.Parameters[ParamName].Value = CommonMethods.Convert_Value(Item_Def.FieldType, Data.Get_EntityValue(Item_Def.FieldName));
                        }
                    }
                }
            }

            //Cmd.ExecuteNonQuery();

            if (SaveDataRow_Process == eSaveDataRow_Process.Process_Insert)
            {
                //Retrieve Output for PK
                if (Prepared.Insert_HasOutput)
                {
                    /*
                    Dictionary<String, Object> PK_Output = new Dictionary<String, Object>();
                    foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
                    {
                        String ParamName = $"@P_{Item_Def.FieldCt}"; //String.Format("@P_{0}", Item_Def.FieldCt);
                        PK_Output.Add(Item_Def.FieldName, Cmd.Parameters[ParamName].Value);
                        //Data.SetPropertyValue(Item_Def.FieldName, ConvertValue(Item_Def.DbType, Item_Def.Is_Nullable, Cmd.Parameters[ParamName].Value));
                        Data.Set_EntityValue(Item_Def.FieldName, ConvertValue(Item_Def.SqlDbType, Item_Def.Is_Nullable, Cmd.Parameters[ParamName].Value));
                    }
                    */

                    DataSet Ds_Output = new DataSet();
                    SQLiteDataAdapter Adp = new SQLiteDataAdapter();
                    Adp.SelectCommand = Cmd;
                    Adp.Fill(Ds_Output);

                    foreach (var Item_Def in TableDef.ToList().Where(O => O.Is_PK))
                    {
                        Data.Set_EntityValue(Item_Def.FieldName, ConvertValue(Item_Def.FieldType, Item_Def.Is_Nullable, Ds_Output.Tables[0].Rows[0][Item_Def.FieldName]));
                    }
                }
                else
                { Cmd.ExecuteNonQuery(); }
            }
            else
            { Cmd.ExecuteNonQuery(); }
        }

        public void DeleteData<T>(
            T Data
            , String TableName = ""
            , SQLiteConnection? Cn = null)
        where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            IList<T> ListData = new List<T>();
            ListData.Add(Data);

            DeleteData(ListData, TableName, Cn);
        }

        public void DeleteData<T>(
            IList<T> Data
            , String TableName = ""
            , SQLiteConnection? Cn = null)
        where T : class, new()
        {
            if (String.IsNullOrEmpty(TableName))
            { TableName = typeof(T).Name; }

            var Converted_Data = DatabaseMapping.ConvertToDataTable(Data);
            DeleteData(Converted_Data, TableName, Cn);
        }

        #endregion

        #region _Methods.Various

        DbType GetDbType(Type DataType)
        {
            var Returned_DbType = DbTypeMap.FirstOrDefault(O => O.Value == DataType).Key;
            return Returned_DbType;
        }

        Boolean Check_TableExists(String TableName, SQLiteConnection? Cn = null)
        {
            String Query =
@"
select count(1) Ct from sqlite_master where name = $P_TableName
";

            List<SQLiteParameter> Params = new List<SQLiteParameter>();
            Params.Add(new SQLiteParameter("$P_TableName", TableName) { DbType = DbType.String, Direction = System.Data.ParameterDirection.Input });
            DataTable Dt = this.ExecuteQuery(Query, Params, Cn: Cn);

            var TableCt = CommonMethods.Convert_Int32(Dt.Rows[0]["Ct"]);

            if (TableCt > 1)
            { throw new Exception($"Multple Tables with the name {TableName} found. Add Schema to Table Name if needed."); }
            else if (TableCt == 1)
            { return true; }
            else
            { return false; }
        }

        TableDef Get_TableDef(String TableName, SQLiteConnection? Cn = null)
        {
            String Query =
@"
Select 
    Tb.name ColumnName
	, substring(Tb.type,1, iif(instr(Tb.type,' ')==0,length(Tb.type),instr(Tb.type,' ')-1)) As DataType
    , Case
		when Tb.""notnull"" = 0 Then 1 
		Else 0 
	End Is_Nullable
    , Tb.pk Is_Pk
from 
	pragma_table_info($P_TableName) Tb
";

            List<SQLiteParameter> Params = new List<SQLiteParameter>();
            Params.Add(new SQLiteParameter("$P_TableName", TableName) { DbType = DbType.String, Direction = System.Data.ParameterDirection.Input });
            DataTable Dt = this.ExecuteQuery(Query, Params, CommandType.Text, Cn);

            TableDef Def = new TableDef();
            Int32 Ct = 0;
            foreach (DataRow Item_Dr in Dt.Rows)
            {
                Ct++;

                Boolean Is_Nullable = CommonMethods.Convert_Boolean(Item_Dr["Is_Nullable"]);
                Boolean Is_PK = CommonMethods.Convert_Boolean(Item_Dr["Is_PK"]);

                Def.Add(new TableDef_Fields()
                {
                    FieldCt = Ct,
                    FieldName = CommonMethods.Convert_String(Item_Dr["ColumnName"]),
                    FieldType = ConvertSqlType(CommonMethods.Convert_String(Item_Dr["DataType"]), Is_Nullable),
                    //DbType = CommonMethods.ParseEnum<SqlDbType>(CommonMethods.Convert_String(Item_Dr["DataType"])),
                    Is_Nullable = Is_Nullable,
                    Is_PK = Is_PK,
                    Is_Identity = Is_PK
                    //,
                    //Length = CommonMethods.Convert_Int32(Item_Dr["Length"]),
                    //Precision = CommonMethods.Convert_Int32(Item_Dr["Precision"]),
                    //Scale = CommonMethods.Convert_Int32(Item_Dr["Scale"])
                });
            }

            return Def;
        }

        Type ConvertSqlType(String SqlType, Boolean Is_Nullable)
        {
            //var DataType = this.SQLiteTypeMap[SqlType];

            var DataType = this.SQLiteTypeMap.FirstOrDefault(O => O.Value == SqlType).Key;
            if (Is_Nullable) { DataType = CommonMethods.GetNullableType(DataType); }

            return DataType;
        }

        Object ConvertValue(String SqlType, Boolean IsNullable, Object Value)
        {
            Type DataType = ConvertSqlType(SqlType, IsNullable);
            return CommonMethods.Convert_Value(DataType, Value);
        }

        Object ConvertValue(Type SqlType, Boolean IsNullable, Object Value)
        {
            return CommonMethods.Convert_Value(SqlType, Value);
        }

        #endregion
    }

    #endregion

    #region _Class.QueryParameters

    [Serializable()]
    public class QueryParameters : List<QueryParameter>
    {
        static QueryParameters() { }

        public String? pQuery { get; set; }

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
        public String? Name { get; set; }
        public Object? Value { get; set; }
        public Type? Type { get; set; }
        public ParameterDirection Direction { get; set; }
    }

    #endregion
}
