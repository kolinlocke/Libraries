using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Commons.EntityProps.EntityHelper;

namespace DataInterfaces.Interfaces
{
    public interface Interface_EntityRepository { }

    public interface Interface_EntityRepository<T_Entity> : Interface_EntityRepository
        where T_Entity : class, new()
    {
        IList<T_Entity> GetList();
        IList<T_Entity> GetList(EntityFields Keys);

        T_Entity GetByID(Int64 ID);
        T_Entity GetByID(String ID);
        T_Entity GetByKeys(EntityFields Keys);

        void Insert(T_Entity Entity);
        void Insert(T_Entity Entity, List<String> Keys);

        void Insert(IList<T_Entity> Entities);
        void Insert(IList<T_Entity> Entities, List<String> Keys);

        void Update(T_Entity Entity);
        void Update(T_Entity Entity, List<String> Keys);

        void Update(IList<T_Entity> Entities);
        void Update(IList<T_Entity> Entities, List<String> Keys);

        void Delete(Int64 ID);
        void Delete(String ID);
        void Delete(EntityFields Keys);
    }
}
