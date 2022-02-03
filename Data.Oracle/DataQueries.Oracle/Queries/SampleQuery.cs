using Data.Oracle.Common;
using DataInterfaces.Common;
using DataInterfaces.Interfaces;
using DataQueries.Oracle.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Commons.EntityProps.Common_Result;

namespace DataQueries.Oracle.Queries
{
    public class SampleQuery : EntityQueryBase<Result_Ct>
    {
        public SampleQuery(Interface_Setup Setup) : base(Setup) { }
        
        public override List<Result_Ct> ExecuteQuery()
        {
            String Query =
@"
Select Count(1) CT
From SampleTable 
";

            return Helpers.ExecuteQuery<Result_Ct>(Query, null);
        }

        public override List<Result_Ct> ExecuteQuery(EntityQueryParameters Parameters)
        {
            return this.ExecuteQuery();
        }
    }
}
