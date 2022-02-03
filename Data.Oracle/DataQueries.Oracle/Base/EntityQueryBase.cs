using Data.Oracle.Common;
using DataInterfaces.Common;
using DataInterfaces.Interfaces;
using DataQueries.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataQueries.Oracle.Implementations
{
    public abstract class EntityQueryBase<T_Entity> : Interface_EntityQuery<T_Entity>
        where T_Entity : class, new()
    {
        Setup mSetup;

        protected Setup pSetup { get { return this.mSetup; } }

        public EntityQueryBase(Interface_Setup Setup)
        { this.mSetup = (Setup)Setup; }

        public abstract List<T_Entity> ExecuteQuery();
        public abstract List<T_Entity> ExecuteQuery(EntityQueryParameters Parameters);

        protected List<T_Entity> Helpers_ExecuteQuery(String Query, EntityQueryParameters Parameters)
        {
            return Helpers.ExecuteQuery<T_Entity>(Query, Parameters, this.pSetup.pCn);
        }
    }
}
