using Commons.EntityProps;
using Data.SQLServer.Common;
using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Data.SQLServer.Common.SQLServerManager;

namespace Data.SQLServer.Implementations
{
    public class EntityRepository<T_Entity> : Interface_EntityRepository<T_Entity>
        where T_Entity : class, new()
    {
        Setup mSetup;

        public EntityRepository(Interface_Setup Setup)
        { this.mSetup = (Setup)Setup; }

        public T_Entity GetByID(long ID)
        {
            var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
            var QueryName = EntityConfig.QueryName;
            var EntityName = EntityConfig.EntityName;
            var Key = EntityHelper.Get_EntityFields<T_Entity>().Where(O => O.IsKey).FirstOrDefault();

            String Query_Key = $" And Tb.{Key.FieldName} = @P_ID "; //String.Format("And Tb.{0} = @P_ID", Key.FieldName, ID);

            String Query =
$@"
Select Tb.* 
From 
    {(String.IsNullOrEmpty(QueryName) ? EntityName : QueryName)} Tb
Where
    1 = 1
    {Query_Key}
";

            QueryParameters Params = new QueryParameters();
            Params.Add("@P_ID", typeof(Int64), ID);

            //var Entity = OracleDBHelper.ExecuteQuery<T_Entity>(Query, Params).FirstOrDefault();
            var Entity = this.mSetup.pCn.ExecuteQuery<T_Entity>(Query, Params).FirstOrDefault();

            return Entity;
        }

        public T_Entity GetByID(string ID)
        {
            var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
            var QueryName = EntityConfig.QueryName;
            var EntityName = EntityConfig.EntityName;
            var Key = EntityHelper.Get_EntityFields<T_Entity>().Where(O => O.IsKey).FirstOrDefault();

            String Query_Key = $" And Tb.{Key.FieldName} = @P_ID ";

            String Query =
$@"
Select Tb.* 
From 
    {(String.IsNullOrEmpty(QueryName) ? EntityName : QueryName)} Tb
Where
    1 = 1
    {Query_Key}
";

            QueryParameters Params = new QueryParameters();
            Params.Add("@P_ID", typeof(String), ID);

            var Entity = this.mSetup.pCn.ExecuteQuery<T_Entity>(Query, Params).FirstOrDefault();

            return Entity;
        }

        public T_Entity GetByKeys(EntityHelper.EntityFields Keys)
        {
            String Query;
            QueryParameters Params;
            this.PrepareQuery(Keys, true, out Query, out Params);

            //var Entity = OracleDBHelper.ExecuteQuery<T_Entity>(Query, Params).FirstOrDefault();
            var Entity = this.mSetup.pCn.ExecuteQuery<T_Entity>(Query, Params).FirstOrDefault();

            return Entity;
        }

        public IList<T_Entity> GetList()
        {
            var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
            var QueryName = EntityConfig.QueryName;
            var EntityName = EntityConfig.EntityName;

            String Query =
$@"
Select Tb.* 
From 
    {(String.IsNullOrEmpty(QueryName) ? EntityName : QueryName)} Tb
";

            //var QueryResult = OracleDBHelper.ExecuteQuery<T_Entity>(Query);
            var QueryResult = this.mSetup.pCn.ExecuteQuery<T_Entity>(Query);

            return QueryResult;
        }

        public IList<T_Entity> GetList(EntityHelper.EntityFields Keys)
        {
            String Query;
            QueryParameters Params;
            this.PrepareQuery(Keys, false, out Query, out Params);

            //var QueryResult = OracleDBHelper.ExecuteQuery<T_Entity>(Query, Params);
            var QueryResult = this.mSetup.pCn.ExecuteQuery<T_Entity>(Query, Params);

            return QueryResult;
        }

        public void Insert(T_Entity Entity)
        {
            this.Insert(Entity, null);
        }

        public void Insert(T_Entity Entity, List<string> Keys)
        {
            this.SaveEntity(eSaveData_Process.Process_Insert, Entity, Keys);
        }

        public void Insert(IList<T_Entity> Entities)
        {
            this.Insert(Entities, null);
        }

        public void Insert(IList<T_Entity> Entities, List<string> Keys)
        {
            this.SaveEntities(eSaveData_Process.Process_Insert, Entities, Keys);
        }

        public void Update(T_Entity Entity)
        {
            this.Update(Entity, null);
        }

        public void Update(T_Entity Entity, List<string> Keys)
        {
            this.SaveEntity(eSaveData_Process.Process_Update, Entity, Keys);
        }

        public void Update(IList<T_Entity> Entities)
        {
            this.Update(Entities, null);
        }

        public void Update(IList<T_Entity> Entities, List<string> Keys)
        {
            this.SaveEntities(eSaveData_Process.Process_Update, Entities, Keys);
        }

        public void Delete(long ID)
        {
            var Entity = this.GetByID(ID);
            if (Entity != null)
            {
                //var EntityName = EntityHelper.Get_EntityName<T_Entity>();
                var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
                var EntityName = EntityConfig.EntityName;
                this.mSetup.pCn.DeleteData(Entity, EntityName);
            }
        }

        public void Delete(string ID)
        {
            var Entity = this.GetByID(ID);
            if (Entity != null)
            {
                //var EntityName = EntityHelper.Get_EntityName<T_Entity>();
                var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
                var EntityName = EntityConfig.EntityName;
                this.mSetup.pCn.DeleteData(Entity, EntityName);
            }
        }

        public void Delete(EntityHelper.EntityFields Keys)
        {
            var Entities = this.GetList(Keys);
            if (Entities != null)
            {
                //var EntityName = EntityHelper.Get_EntityName<T_Entity>();
                var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
                var EntityName = EntityConfig.EntityName;
                this.mSetup.pCn.DeleteData(Entities, EntityName);
            }
        }

        //[-]

        void SaveEntity(
          eSaveData_Process Process
          , T_Entity Entity
          , IList<String> Keys)
        {
            //var EntityName = EntityHelper.Get_EntityName<T_Entity>();
            var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
            var EntityName = EntityConfig.EntityName;

            EntityKeys EntityKeys = null;
            if (Keys != null)
            {
                EntityKeys = new EntityKeys();
                Keys.ToList().ForEach(O =>
                {
                    EntityKeys.Add(new EntityKey() { Name = O });
                });
            }

            //OracleDBHelper.SaveData<T_Entity>(Process, ref Entity, EntityName, Keys: EntityKeys);
            this.mSetup.pCn.SaveData<T_Entity>(Process, ref Entity, EntityName, Keys: EntityKeys);
        }

        void SaveEntities(
          SQLServerManager.eSaveData_Process Process
          , IList<T_Entity> Entities
          , IList<String> Keys)
        {
            //var EntityName = EntityHelper.Get_EntityName<T_Entity>();
            var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
            var EntityName = EntityConfig.EntityName;

            EntityKeys EntityKeys = null;
            if (Keys != null)
            {
                EntityKeys = new EntityKeys();
                Keys.ToList().ForEach(O =>
                {
                    EntityKeys.Add(new EntityKey() { Name = O });
                });
            }

            //OracleDBHelper.SaveData<T_Entity>(Process, ref Entities, EntityName, Keys: EntityKeys);
            this.mSetup.pCn.SaveData<T_Entity>(Process, ref Entities, EntityName, Keys: EntityKeys);
        }

        void PrepareQuery(
          EntityHelper.EntityFields Keys
          , Boolean Is_GetOneOnly
          , out string Query
          , out QueryParameters Params)
        {
            var EntityConfig = EntityHelper.Get_EntityConfig<T_Entity>();
            var QueryName = EntityConfig.QueryName;
            var EntityName = EntityConfig.EntityName;
            var EntityKeys =
                Keys
                    .Select(O_Key =>
                    {
                        String FieldName = $"Tb.{O_Key.FieldName}";
                        String ParameterIndex = $"@P_{Keys.IndexOf(O_Key)}";
                        String NullDefaultIndex = $"@P_Nd_{Keys.IndexOf(O_Key)}";

                        if (O_Key.Field_IgnoreCase)
                        {
                            FieldName = $"Upper({FieldName})";
                            ParameterIndex = $"Upper({ParameterIndex})";
                        }

                        if (O_Key.Field_Trim)
                        {
                            FieldName = $"Trim({FieldName})";
                            ParameterIndex = $"Trim({ParameterIndex})";
                        }

                        if (O_Key.Field_Nullable)
                        {
                            FieldName = $"IsNull({FieldName},{NullDefaultIndex})";
                        }

                        return $"{FieldName} = {ParameterIndex}";
                    })
                    .ToList();

            String Query_GetOne = "";
            if (Is_GetOneOnly)
            { Query_GetOne = "Top 1"; }

            //String Query_Keys = String.Format(" And {0} ", String.Join(" And ", EntityKeys));
            String Query_Keys = $" And {String.Join(" And ", EntityKeys)} ";

            Query = $@"
Select {Query_GetOne} Tb.* 
From 
    {(String.IsNullOrEmpty(QueryName) ? EntityName : QueryName)} Tb
Where
    1 = 1    
    {Query_Keys}
";
            Params = new QueryParameters();
            Params.AddRange(
                Keys.Select(O =>
                    new QueryParameter()
                    {
                        Name = $"@P_{Keys.IndexOf(O)}",
                        Value = O.Value,
                        Direction = ParameterDirection.Input
                    })
                .ToList());

            Params.AddRange(
                Keys
                .Where(O => O.Field_Nullable)
                .Select(O =>
                    new QueryParameter()
                    {
                        Name = $"@P_Nd_{Keys.IndexOf(O)}",
                        Value = O.Field_NullDefaultValue,
                        Direction = ParameterDirection.Input
                    })
                .ToList());
        }
    }
}
