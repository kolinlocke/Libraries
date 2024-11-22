using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Commons.Serializers;

namespace Commons.EntityProps
{
    public static class EntityHelper
    {
        #region _Variables

        public static EntityConfig pEntityConfig { get; set; }

        #endregion

        #region _Definitions

        [Serializable()]
        [DataContract()]
        public class EntityFields : List<EntityField>
        {
            public void Add(
                String FieldName
                , Object FieldValue
                , Boolean Field_IgnoreCase = false
                , Boolean Field_Nullable = false
                , Object Field_NullDefaultValue = null)
            {
                this.Add(
                    new EntityField()
                    {
                        FieldName = FieldName,
                        Value = FieldValue,
                        Field_IgnoreCase = Field_IgnoreCase,
                        Field_Nullable = Field_Nullable,
                        Field_NullDefaultValue = Field_NullDefaultValue
                    });
            }
        }

        [Serializable()]
        [DataContract()]
        public class EntityField
        {
            [DataMember()]
            public String FieldName { get; set; }

            [DataMember()]
            public String PropertyName { get; set; }

            [DataMember()]
            public Object Value { get; set; }

            [DataMember()]
            public Boolean Field_IgnoreCase { get; set; }

            [DataMember()]
            public Boolean Field_Trim { get; set; }

            [DataMember()]
            public Boolean Field_Nullable { get; set; }

            [DataMember()]
            public Object Field_NullDefaultValue { get; set; }

            [DataMember()]
            public Boolean IsKey { get; set; }

            [DataMember()]
            public Boolean IsExcluded { get; set; }

            public PropertyInfo PropertyInfo { get; set; }
        }

        #endregion

        #region _Methods

        public static void Setup_EntityConfig(String FilePath)
        {
            if (String.IsNullOrEmpty(FilePath))
            {
                pEntityConfig = null;
                return;
            }

            var EntityConfig = Serializer.DeserializeFromFile<EntityConfig>(Serializer.SerializerType.Xml, FilePath);
            pEntityConfig = EntityConfig;
        }

        public static EntityConfigItem Get_EntityConfig<T_Entity>()
        {
            EntityConfigItem EntityConfig = null;

            if (pEntityConfig != null)
            {
                Type EntityType = typeof(T_Entity);
                EntityConfig = pEntityConfig.ConfigItems.FirstOrDefault(O => O.EntityType == EntityType.Name);
            }

            if (EntityConfig == null)
            {
                Type EntityType = typeof(T_Entity);
                EntityConfig = new EntityConfigItem();
                EntityConfig.EntityType = EntityType.Name;
                EntityConfig.EntityName = Get_EntityName<T_Entity>();
                EntityConfig.QueryName = Get_QueryName<T_Entity>();
            }

            return EntityConfig;
        }

        public static String Get_EntityName<T>()
        {
            String EntityName = "";
            Type EntityType = typeof(T);
            var EntityAtt = EntityType.GetCustomAttributes(typeof(EntityAttribute), true);
            if (EntityAtt.Any())
            { EntityName = (EntityAtt.FirstOrDefault() as EntityAttribute).EntityName; }

            return EntityName;
        }

        public static String Get_EntityName(Type EntityType)
        {
            String EntityName = "";
            var EntityAtt = EntityType.GetCustomAttributes(typeof(EntityAttribute), true);
            if (EntityAtt.Any())
            { EntityName = (EntityAtt.FirstOrDefault() as EntityAttribute).EntityName; }

            return EntityName;
        }

        public static String Get_QueryName<T>()
        {
            String QueryName = "";
            Type EntityType = typeof(T);
            var EntityAtt = EntityType.GetCustomAttributes(typeof(EntityAttribute), true);
            if (EntityAtt.Any())
            { QueryName = (EntityAtt.FirstOrDefault() as EntityAttribute).QueryName; }

            return QueryName;
        }

        public static String Get_QueryName(Type EntityType)
        {
            String QueryName = "";
            var EntityAtt = EntityType.GetCustomAttributes(typeof(EntityAttribute), true);
            if (EntityAtt.Any())
            { QueryName = (EntityAtt.FirstOrDefault() as EntityAttribute).QueryName; }

            return QueryName;
        }

        public static EntityFields Get_EntityFields<T>()
        {
            /*
            EntityFields EntityFields = new EntityFields();
            Type EntityType = typeof(T);

            var EntityProperties = EntityType.GetProperties().ToList();
            EntityProperties.ForEach(O_EntityProperty =>
            {
                String FieldName = O_EntityProperty.Name;
                Boolean IsKey = false;
                Boolean IsExcluded = false;

                var O_FieldAtt =
                    O_EntityProperty
                        .GetCustomAttributes(typeof(EntityFieldAttribute), true)
                        .Select(O => (EntityFieldAttribute)O)
                        .FirstOrDefault();

                if (O_FieldAtt != null)
                {
                    if (!String.IsNullOrEmpty(O_FieldAtt.FieldName))
                    { FieldName = O_FieldAtt.FieldName; }
                    IsKey = O_FieldAtt.IsKey;
                    IsExcluded = O_FieldAtt.IsExcluded;
                }

                EntityField Ef = new EntityField();
                Ef.FieldName = FieldName;
                Ef.PropertyName = O_EntityProperty.Name;
                Ef.IsKey = IsKey;
                Ef.IsExcluded = IsExcluded;
                Ef.PropertyInfo = O_EntityProperty;

                EntityFields.Add(Ef);
            });

            return EntityFields;
        */
            return Get_EntityFields(typeof(T));

        }

        public static EntityFields Get_EntityFields(Type TypeObj)
        {
            EntityFields EntityFields = new EntityFields();
            Type EntityType = TypeObj;

            var EntityProperties = EntityType.GetProperties().ToList();
            EntityProperties.ForEach(O_EntityProperty =>
            {
                String FieldName = O_EntityProperty.Name;
                Boolean IsKey = false;
                Boolean IsExcluded = false;

                var O_FieldAtt =
                    O_EntityProperty
                        .GetCustomAttributes(typeof(EntityFieldAttribute), true)
                        .Select(O => (EntityFieldAttribute)O)
                        .FirstOrDefault();

                if (O_FieldAtt != null)
                {
                    if (!String.IsNullOrEmpty(O_FieldAtt.FieldName))
                    { FieldName = O_FieldAtt.FieldName; }
                    IsKey = O_FieldAtt.IsKey;
                    IsExcluded = O_FieldAtt.IsExcluded;
                }

                EntityField Ef = new EntityField();
                Ef.FieldName = FieldName;
                Ef.PropertyName = O_EntityProperty.Name;
                Ef.IsKey = IsKey;
                Ef.IsExcluded = IsExcluded;
                Ef.PropertyInfo = O_EntityProperty;

                EntityFields.Add(Ef);
            });

            return EntityFields;
        }

        public static EntityFields Get_EntityKeys<T>(this T obj) where T : class
        {
            var Keys =
                Get_EntityFields<T>()
                    .Where(O => O.IsKey = true)
                    .Select(O => new EntityField() { FieldName = O.FieldName, PropertyName = O.PropertyName, IsKey = O.IsKey, Value = obj.GetPropertyValue(O.PropertyName) })
                    .ToList();

            EntityFields Returned = new EntityFields();
            Returned.AddRange(Keys);

            return Returned;
        }

        public static Object Get_EntityValue<T>(this T Entity, String FieldName) where T : class
        {
            var EntityFields = Get_EntityFields<T>();
            var EntityField = EntityFields.FirstOrDefault(O => O.FieldName == FieldName);
            if (EntityField != null)
            { return EntityField.PropertyInfo.GetValue(Entity); }
            else
            { return null; }
        }

        public static Object Get_EntityValue(this Object Entity, String FieldName)
        {
            var EntityType = Entity.GetType();
            var EntityFields = Get_EntityFields(EntityType);
            var EntityField = EntityFields.FirstOrDefault(O => O.FieldName == FieldName);
            if (EntityField != null)
            { return EntityField.PropertyInfo.GetValue(Entity); }
            else
            { return null; }
        }
        public static void Set_EntityValue<T>(this T Entity, String FieldName, Object Value) where T : class
        {
            var EntityFields = Get_EntityFields<T>();
            var EntityField = EntityFields.FirstOrDefault(O => O.FieldName == FieldName);
            if (EntityField != null)
            {
                if (EntityField.PropertyInfo.CanWrite)
                {
                    Object ConvertedValue = CommonMethods.Convert_Value(EntityField.PropertyInfo.PropertyType, Value);
                    EntityField.PropertyInfo.SetValue(Entity, ConvertedValue, null);
                }
            }
        }

        public static void Set_EntityValue(this Object Entity, String FieldName, Object Value)
        {
            var EntityFields = Get_EntityFields(Entity.GetType());
            var EntityField = EntityFields.FirstOrDefault(O => O.FieldName == FieldName);
            if (EntityField != null)
            {
                if (EntityField.PropertyInfo.CanWrite)
                {
                    Object ConvertedValue = CommonMethods.Convert_Value(EntityField.PropertyInfo.PropertyType, Value);
                    EntityField.PropertyInfo.SetValue(Entity, ConvertedValue, null);
                }
            }
        }

        public static Boolean Has_EntityField<T>(this T Entity, String FieldName)
        {
            var EntityFields = Get_EntityFields<T>();
            var Has_Field = EntityFields.Any(O => O.FieldName == FieldName);

            return Has_Field;
        }

        #endregion
    }
}