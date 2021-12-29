using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class EntityMapping
    {
        public static IList<T_Target> MapEntity<T_Source, T_Target>(IList<T_Source> Source)
            where T_Target : class, new()
            where T_Source : class, new()
        {
            List<T_Target> Target = new List<T_Target>();
            var Source_Properties = typeof(T_Source).GetProperties().ToList();
            var Target_Properties = typeof(T_Target).GetProperties().ToList();
            Source.ToList().ForEach(Item_Source => {
                T_Target Item_Target = new T_Target();
                Item_Target =
                    MapEntity<T_Target, T_Source>(
                        Item_Source
                        , Item_Target
                        , Source_Properties
                        , Target_Properties);

                Target.Add(Item_Target);
            });

            return Target;
        }

        public static IList<T_Target> MapEntity<T_Target>(IList<T_Target> Source)
            where T_Target : class, new()
        {
            return MapEntity<T_Target, T_Target>(Source);
        }

        public static List<T_Target> MapEntity<T_Target>(List<T_Target> Source)
            where T_Target : class, new()
        {
            return MapEntity((IList<T_Target>)Source).ToList();
        }

        public static T_Target MapEntity<T_Target>(T_Target Source)
            where T_Target : class, new()
        {
            T_Target Target = new T_Target();
            return MapEntity<T_Target, T_Target>(Source, Target);
        }

        public static T_Target MapEntity<T_Source, T_Target>(T_Source Source)
            where T_Source : class, new()
            where T_Target : class, new()
        {
            T_Target Target = new T_Target();
            return MapEntity(Source, Target);
        }

        public static T_Target MapEntity<T_Source, T_Target>(T_Source Source, T_Target Target)
            where T_Source : class, new()
            where T_Target : class, new()
        {
            var Source_Properties = typeof(T_Source).GetProperties().ToList();
            var Target_Properties = typeof(T_Target).GetProperties().ToList();

            Target =
                MapEntity<T_Target, T_Source>(
                    Source
                    , Target
                    , Source_Properties
                    , Target_Properties);

            return Target;
        }

        static T_Target MapEntity<T_Target, T_Source>(
            T_Source Source
            , T_Target Target
            , List<PropertyInfo> Source_Properties
            , List<PropertyInfo> Target_Properties)
            where T_Target : class, new()
            where T_Source : class, new()
        {
            Source_Properties.ForEach(Item_Source_Property => {
                var Source_Value = Item_Source_Property.GetValue(Source, null);
                var Target_Property = Target_Properties.FirstOrDefault(O => O.Name.ToUpper() == Item_Source_Property.Name.ToUpper());
                if (Target_Property != null)
                {
                    if (
                        Item_Source_Property.PropertyType.IsGenericType
                        && Target_Property.PropertyType.IsGenericType
                        && typeof(IList<>).IsAssignableFrom(Item_Source_Property.PropertyType.GetGenericTypeDefinition())
                        && typeof(IList<>).IsAssignableFrom(Target_Property.PropertyType.GetGenericTypeDefinition()))
                    {
                        if (Source_Value != null)
                        {
                            MethodInfo Method_MapViewModel =
                                typeof(EntityMapping)
                                    .GetGenericMethod("MapEntity", new Type[] { typeof(IList<>) })
                                    .MakeGenericMethod(
                                        Target_Property.PropertyType.GetGenericArguments().First()
                                        , Item_Source_Property.PropertyType.GetGenericArguments().First());

                            Object Target_Data = Method_MapViewModel.Invoke(null, new Object[] { Source_Value });
                            Target_Property.SetValue(Target, Target_Data, null);
                        }
                    }
                    else
                    {
                        if (Target_Property.CanWrite)
                        { Target_Property.SetValue(Target, Source_Value, null); }
                    }
                }
            });

            return Target;
        }
    }
}
