using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataInterfaces.Common
{
    [Serializable()]
    [DataContract()]
    public class EntityQueryParameters : List<EntityQueryParameter>
    {
        public EntityQueryParameter this[String ParameterName]
        {
            get { return this.FirstOrDefault(O => O.ParameterName == ParameterName); }
        }

        public void Add(
            String ParameterName
            , Type ParameterType
            , Object ParameterValue)
        {
            this.Add(
                new EntityQueryParameter()
                {
                    ParameterName = ParameterName,
                    ParameterType = ParameterType,
                    ParameterValue = ParameterValue,
                    ParameterDirection = ParameterDirection.Input
                });
        }

        public void Add<T>(
            String ParameterName
            , T ParameterValue)
        {
            this.Add(ParameterName, typeof(T), ParameterValue);
        }

        public EntityQueryParameter Get_Param(String ParameterName)
        {
            var Param = this[ParameterName];
            if (Param == null) { return null; }

            EntityQueryParameter Returned_Param =
                new EntityQueryParameter()
                {
                    ParameterName = Param.ParameterName,
                    ParameterValue = Param.ParameterValue,
                    ParameterType = Param.ParameterType,
                    ParameterDirection = Param.ParameterDirection
                };

            return Param;
        }
    }

    [Serializable()]
    [DataContract()]
    public class EntityQueryParameter
    {
        [DataMember()]
        public String ParameterName { get; set; }

        [DataMember()]
        public Object ParameterValue { get; set; }

        [DataMember()]
        public Type ParameterType { get; set; }

        [DataMember()]
        public ParameterDirection ParameterDirection { get; set; }
    }
}
