using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Data.Entity;
//using System.Data.Objects;
//using System.Data.Entity.Infrastructure;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Commons
{
    public static class ExtensionMethods
    {
        public static ObservableCollection<T> Remove<T>(
        this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return coll;

        }

        /*Code from http://forums.asp.net/t/1519839.aspx/1 */
        /// <summary>
        /// First tick of the day.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime DayMin(this DateTime date)
        {
            return date.Date;   // minimum of this day
        }

        /// <summary>
        /// Last tick of the day.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static DateTime DayMax(this DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);   // last tick of this day
        }

        /// <summary>
        /// Returns a IEnumerable DateTime from this Date to DateTo
        /// </summary>
        /// <param name="dateFrom">This DateTime</param>
        /// <param name="dateTo">Date to end</param>
        /// <param name="dayInterval">Day interval count. Default is 1.</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> EachDayTo(this DateTime dateFrom, DateTime dateTo, Int32 dayInterval = 1)
        {
            return CommonMethods.EachDay(dateFrom, dateTo, dayInterval);
        }

        public static IEnumerable<DateTime> EachDayWithDaysTo(
            this DateTime dateFrom
            , DateTime dateTo
            , Boolean IsMon
            , Boolean IsTue
            , Boolean IsWed
            , Boolean IsThu
            , Boolean IsFri
            , Boolean IsSat
            , Boolean IsSun)
        {
            return CommonMethods.EachDayWithDays(dateFrom, dateTo, IsMon, IsTue, IsWed, IsThu, IsFri, IsSat, IsSun);
        }

        /// <summary>
        /// Returns a IEnumerable DateTime from this Date to DateTo, months at a time
        /// </summary>
        /// <param name="dateFrom">This DateTime</param>
        /// <param name="dateTo">Date to end</param>
        /// <param name="monthInterval">Month interval count. Default is 1.</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> EachMonthTo(this DateTime dateFrom, DateTime dateTo, Int32 monthInterval = 1)
        {
            return CommonMethods.EachMonth(dateFrom, dateTo, monthInterval);
        }

        public static Boolean HasProperty<T>(this T obj, string propertyName) where T : class
        {
            var Obj_Type = obj.GetType().GetProperty(propertyName);
            Boolean HasProperty = Obj_Type != null;
            return HasProperty;
        }

        public static Boolean HasPropertyValue<T>(this T obj, String PropertyName) where T : class
        {
            var Obj_Property = obj.GetType().GetProperty(PropertyName);
            if (Obj_Property != null && Obj_Property.CanWrite)
            {
                Object Obj_Value = Obj_Property.GetValue(obj);
                Object ConvertedValue = CommonMethods.Convert_Value(Obj_Property.PropertyType, Obj_Value);

                if (CommonMethods.HasValue(ConvertedValue))
                { return true; }
                else
                { return false; }
            }

            return false;
        }

        public static object GetPropertyValue<T>(this T obj, string propertyName) where T : class
        {
            var Obj_Type = obj.GetType().GetProperty(propertyName);
            if (Obj_Type != null)
            { return Obj_Type.GetValue(obj, null); }
            else
            { return default(T); }
        }

        public static void SetPropertyValue<T>(this T obj, String PropertyName, Object PropertyValue) where T : class
        {
            var Obj_Property = obj.GetType().GetProperty(PropertyName);
            if (Obj_Property != null && Obj_Property.CanWrite)
            {
                Object ConvertedValue = CommonMethods.Convert_Value(Obj_Property.PropertyType, PropertyValue);
                Obj_Property.SetValue(obj, ConvertedValue, null);
            }
        }

        public static Type GetPropertyType<T>(this T obj, string propertyName) where T : class
        {
            return obj.GetType().GetProperty(propertyName).PropertyType;
        }

        public static List<String> GetProperties<T>(this T obj) where T : class
        {
            var Props = obj.GetType().GetProperties().ToList();
            var Returned =  Props.Select(O_Prop => O_Prop.Name).ToList();
            return Returned;
        }

        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                    x.Namespace == y.Namespace &&
                    x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }
    }

    public static class TreeToEnumerableEx
    {
        public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc)
        {
            yield return head;
            foreach (var node in childrenFunc(head))
            {
                foreach (var child in AsDepthFirstEnumerable(node, childrenFunc))
                {
                    yield return child;
                }
            }
        }

        public static IEnumerable<T> AsBreadthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc)
        {
            yield return head;
            var last = head;
            foreach (var node in AsBreadthFirstEnumerable(head, childrenFunc))
            {
                foreach (var child in childrenFunc(node))
                {
                    yield return child;
                    last = child;
                }
                if (last.Equals(node))
                    yield break;
            }
        }
    }
}
