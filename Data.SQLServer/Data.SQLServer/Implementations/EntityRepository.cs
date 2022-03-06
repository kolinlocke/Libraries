using Commons.EntityProps;
using Data.SQLServer.Common;
using DataInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            String Query_Key = String.Format("And Tb.{0} = @P_ID", Key.FieldName, ID);

            String Query =
$@"
Select Tb.* 
From 
    ({(String.IsNullOrEmpty(QueryName) ? EntityName : QueryName)}) Tb
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
            throw new NotImplementedException();
        }

        public T_Entity GetByKeys(EntityHelper.EntityFields Keys)
        {
            throw new NotImplementedException();
        }

        public IList<T_Entity> GetList()
        {
            throw new NotImplementedException();
        }

        public IList<T_Entity> GetList(EntityHelper.EntityFields Keys)
        {
            throw new NotImplementedException();
        }

        public void Insert(T_Entity Entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(T_Entity Entity, List<string> Keys)
        {
            throw new NotImplementedException();
        }

        public void Insert(IList<T_Entity> Entities)
        {
            throw new NotImplementedException();
        }

        public void Insert(IList<T_Entity> Entities, List<string> Keys)
        {
            throw new NotImplementedException();
        }

        public void Update(T_Entity Entity)
        {
            throw new NotImplementedException();
        }

        public void Update(T_Entity Entity, List<string> Keys)
        {
            throw new NotImplementedException();
        }

        public void Update(IList<T_Entity> Entities)
        {
            throw new NotImplementedException();
        }

        public void Update(IList<T_Entity> Entities, List<string> Keys)
        {
            throw new NotImplementedException();
        }

        public void Delete(long ID)
        {
            throw new NotImplementedException();
        }

        public void Delete(string ID)
        {
            throw new NotImplementedException();
        }

        public void Delete(EntityHelper.EntityFields Keys)
        {
            throw new NotImplementedException();
        }
    }
}
