using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Commons
{
    public class CommonMethods
    {
        static Object Common_SyncLock = new Object();
        const String Cns_Regex_Numeric = @"(?<!\S)(?=.)(0|([1-9](\d*|\d{0,2}(,\d{3})*)))?(\.\d*[0-9])?(?!\S)";

        #region debugging

        static DateTime debugStart = new DateTime();
        static DateTime debugEnd = new DateTime();
        //static TimeSpan debugDuration = new TimeSpan();

        #endregion
        /// <summary>
        /// Returns true if the object is numeric.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static bool IsNumeric(object obj)
        {
            if (obj == null)
            { return false; }

            TypeCode typeCode = Type.GetTypeCode(obj.GetType());
            switch (typeCode)
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static Boolean IsNumeric(String Input)
        {
            Regex Rx = new Regex(Cns_Regex_Numeric);
            var Rx_Matches = Rx.Matches(Input);
            return Rx_Matches.Count > 0;
        }

        public static Boolean CheckPattern(String Input, String Pattern, RegexOptions Options = RegexOptions.None)
        {
            Regex Rx = new Regex(Pattern, Options);
            var Rx_Matches = Rx.Matches(Convert_String(Input));
            return Rx_Matches.Count > 0;
        }

        /// <summary>
        /// Checks the input objects and returns a default if it is a null, this includes for DBNull checking
        /// </summary>
        /// <param name="Obj_Input">
        /// The input object
        /// </param>
        /// <param name="Obj_NullOutput">
        /// The default output
        /// </param>
        /// <returns></returns>
        public static object IsNull(object Obj_Input, object Obj_NullOutput)
        {
            if (Obj_Input == null || Object.Equals(DBNull.Value, Obj_Input))
            { return Obj_NullOutput; }
            else
            { return Obj_Input; }
        }

        /// <summary>
        /// Converts an object to a double data type without exceptions.
        /// If conversion fails, returns 0.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <returns></returns>
        public static double Convert_Double(object Value)
        { return Convert_Double(Value, 0); }

        /// <summary>
        /// Converts an object to a double data type without exceptions.
        /// If conversion fails, returns the specified default value.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <param name="DefaultValue">
        /// The value to be used if the conversion fails.
        /// </param>
        /// <returns></returns>
        public static double Convert_Double(Object Value, double DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            double ReturnValue;
            if (!double.TryParse(ValueString, out ReturnValue))
            { ReturnValue = DefaultValue; }

            return ReturnValue;
        }

        public static decimal Convert_Decimal(object Value)
        { return Convert_Decimal(Value, 0); }

        public static decimal Convert_Decimal(Object Value, decimal DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            decimal ReturnValue;
            if (!decimal.TryParse(ValueString, out ReturnValue))
            { ReturnValue = DefaultValue; }

            return ReturnValue;
        }

        public static Int16 Convert_Int16(Object Value)
        { return Convert_Int16(Value, 0); }

        public static Int16 Convert_Int16(Object Value, Int16 DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            Int16 ReturnValue;
            if (!Int16.TryParse(ValueString, out ReturnValue))
            { ReturnValue = DefaultValue; }
            return ReturnValue;
        }

        /// <summary>
        /// Converts an object to a Int32 data type without exceptions.
        /// If conversion fails, returns 0.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <returns></returns>
        public static Int32 Convert_Int32(Object Value)
        { return Convert_Int32(Value, 0); }

        /// <summary>
        /// Converts an object to a Int32 data type without exceptions.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <param name="DefaultValue">
        /// The value to be used if the conversion fails.
        /// </param>
        /// <returns></returns>
        public static Int32 Convert_Int32(Object Value, Int32 DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            Int32 ReturnValue;
            if (!Int32.TryParse(ValueString, out ReturnValue))
            { ReturnValue = DefaultValue; }
            return ReturnValue;
        }

        /// <summary>
        /// Converts an object to a Int64 data type without exceptions.
        /// If conversion fails, returns 0.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <returns></returns>
        public static Int64 Convert_Int64(Object Value)
        { return Convert_Int64(Value, 0); }

        /// <summary>
        /// Converts an object to a Int64 data type without exceptions.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <param name="DefaultValue">
        /// The value to be used if the conversion fails.
        /// </param>
        /// <returns></returns>
        public static Int64 Convert_Int64(Object Value, Int64 DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            Int64 ReturnValue;
            if (!Int64.TryParse(ValueString, out ReturnValue))
            { ReturnValue = DefaultValue; }
            return ReturnValue;
        }

        public static Int64? Convert_Int64_Nullable(Object Value)
        { return Convert_Int64_Nullable(Value, null); }

        public static Int64? Convert_Int64_Nullable(Object Value, Int64? DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            Int64? ReturnValue;
            Int64 OutValue;
            if (!Int64.TryParse(ValueString, out OutValue))
            { ReturnValue = DefaultValue; }
            else
            { ReturnValue = OutValue; }

            return ReturnValue;
        }

        public static DateTime? Convert_DateTime(Object Value)
        { return Convert_DateTime(Value, (DateTime?)null); }

        /// <summary>
        /// Converts an object to a Nullable Date Time data type without exceptions.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <param name="DefaultValue">
        /// The value to be used if the conversion fails.
        /// </param>
        /// <returns></returns>
        public static DateTime? Convert_DateTime(Object Value, DateTime? DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            DateTime ReturnValue_Ex;
            DateTime? ReturnValue;
            if (DateTime.TryParse(ValueString, out ReturnValue_Ex))
            { ReturnValue = ReturnValue_Ex; }
            else
            { ReturnValue = DefaultValue; }

            return ReturnValue;
        }

        public static DateTime? Convert_DateTime(Object Value, String DateFormat)
        {
            return Convert_DateTime(Value, DateFormat, (DateTime?)null);
        }

        public static DateTime? Convert_DateTime(Object Value, String DateFormat, DateTime? DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            DateTime ReturnValue_Ex;
            DateTime? ReturnValue;
            if (DateTime.TryParseExact(ValueString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out ReturnValue_Ex))
            { ReturnValue = ReturnValue_Ex; }
            else
            { ReturnValue = DefaultValue; }

            return ReturnValue;
        }

        public static DateTime? Convert_DateTime(Object Value, String[] DateFormats)
        {
            return Convert_DateTime(Value, DateFormats, (DateTime?)null);
        }

        public static DateTime? Convert_DateTime(Object Value, String[] DateFormats, DateTime? DefaultValue)
        {
            string ValueString = string.Empty;
            if (Value != null)
            {
                try
                { ValueString = Value.ToString(); }
                catch { }
            }

            DateTime ReturnValue_Ex;
            DateTime? ReturnValue;
            if (DateTime.TryParseExact(ValueString, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out ReturnValue_Ex))
            { ReturnValue = ReturnValue_Ex; }
            else
            { ReturnValue = DefaultValue; }

            return ReturnValue;
        }

        public static String Convert_String(Object Value)
        { return Convert_String(Value, ""); }

        /// <summary>
        /// Converts an object to a string data type without exceptions.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <param name="DefaultValue">
        /// The value to be used if the conversion fails.
        /// </param>
        /// <returns></returns>
        public static String Convert_String(Object Value, String DefaultValue)
        { return Convert.ToString(IsNull(Value, DefaultValue)); }

        /// <summary>
        /// Converts an object to a boolean data type without exceptions.
        /// If conversion fails, returns false.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static Boolean Convert_Boolean(Object Value)
        { return Convert_Boolean(Value, false); }

        /// <summary>
        /// Converts an object to a boolean data type without exceptions.
        /// If conversion fails, returns the specified default value.
        /// </summary>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <param name="DefaultValue">
        /// The value to be used if the conversion fails.
        /// </param>
        /// <returns></returns>
        public static bool Convert_Boolean(Object Value, bool DefaultValue)
        {
            string ValueString = string.Empty;
            try
            {
                if (Value != null)
                { ValueString = Value.ToString(); }
            }
            catch { }

            bool ReturnValue;
            if (!bool.TryParse(ValueString, out ReturnValue))
            {
                switch (ValueString.ToLower())
                {
                    case "true":
                    case "yes":
                    case "1":
                        {
                            ReturnValue = true;
                            break;
                        }
                    case "false":
                    case "no":
                    case "0":
                        {
                            ReturnValue = false;
                            break;
                        }
                    default:
                        {
                            ReturnValue = DefaultValue;
                            break;
                        }
                }
            }
            return ReturnValue;
        }

        public static Byte Convert_Byte(Object Value)
        { return Convert_Byte(Value, 0); }

        public static Byte Convert_Byte(Object Value, Byte DefaultValue)
        {
            string ValueString = string.Empty;
            try
            { ValueString = Value.ToString(); }
            catch { }

            Byte ReturnValue;
            if (!Byte.TryParse(ValueString, out ReturnValue))
            { ReturnValue = DefaultValue; }
            return ReturnValue;
        }

        public static Int32 Convert_MonthName(String MonthName)
        {
            Int32 Return_Value = 0;
            DateTime Out_Value;
            if (DateTime.TryParseExact(MonthName, new String[] { "MMM", "MMMM" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out Out_Value))
            { Return_Value = Out_Value.Month; }
            else
            { throw new Exception("Invalid Month Name"); }

            return Return_Value;
        }

        public static DateTime GetTimePart(DateTime Input)
        { return new DateTime(1, 1, 1, Input.Hour, Input.Minute, Input.Second); }

        public static T ParseEnum<T>(String Value)
            where T : struct, IComparable, IFormattable
        { return ParseEnum<T>(Value, default(T)); }

        public static T ParseEnum<T>(Int32 Value)
            where T : struct, IComparable, IFormattable
        { return ParseEnum<T>(Value, default(T)); }

        public static T ParseEnum<T>(Object Value, T DefaultValue)
            where T : struct, IComparable, IFormattable
        {
            //if (Enum.IsDefined(typeof(T), Value))
            //{ return (T)Enum.Parse(typeof(T), Value.ToString(), true); }
            //return DefaultValue;


            //var Parsed = (T)Enum.Parse(typeof(T), Value.ToString(), true);
            //if (!Object.Equals(Parsed, DefaultValue))
            //{ return Parsed; }
            //else
            //{ return DefaultValue; }

            Object Parsed = null;
            if (Enum.TryParse(typeof(T), Value.ToString(), true, out Parsed))
            { return (T)Parsed; }
            else
            { return DefaultValue; }
        }

        public static object GetDefault(Type t)
        {
            return typeof(CommonMethods).GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(null, null);
        }

        public static T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        public static Object Convert_Value(Type Type, Object Value)
        {
            MethodInfo Method =
                typeof(CommonMethods)
                    .GetMethod("Convert_Value", new Type[] { typeof(Object) })
                    .MakeGenericMethod(Type);

            Object Returned = Method.Invoke(null, new Object[] { Value });

            return Returned;
        }

        public static Object Convert_Value(Type Type, Object Value, Object DefaultValue)
        {
            MethodInfo Method =
                typeof(CommonMethods)
                    .GetMethod("Convert_Value", new Type[] { typeof(Object), typeof(Object) })
                    .MakeGenericMethod(Type);

            Object Returned = Method.Invoke(null, new Object[] { Value, DefaultValue });

            return Returned;
        }

        /// <summary>
        /// Converts an object to a specified data type.
        /// If conversion fails, returns the default value.
        /// </summary>
        /// <typeparam name="T">
        /// Accepts String, Int16, Int32, Int64, Decimal, Float, Single, Double, DateTime, Boolean, Byte.
        /// </typeparam>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <returns>
        /// Returns the converted value according to specified type.
        /// </returns>
        public static T Convert_Value<T>(Object Value)
        {
            dynamic Dynamic_DefaultValue;

            Type TypeCheck = typeof(T);
            if (TypeCheck.IsGenericType && TypeCheck.GetGenericTypeDefinition() == typeof(Nullable<>))
            { Dynamic_DefaultValue = null; }
            else if (typeof(T) == typeof(Int16) || typeof(T) == typeof(Int16?))
            { Dynamic_DefaultValue = Convert.ToInt16(0); }
            else if (typeof(T) == typeof(Int32) || typeof(T) == typeof(Int32?))
            { Dynamic_DefaultValue = Convert.ToInt32(0); }
            else if (typeof(T) == typeof(Int64))
            { Dynamic_DefaultValue = Convert.ToInt64(0); }
            else if (typeof(T) == typeof(Int64?))
            { Dynamic_DefaultValue = null; }
            else if (typeof(T) == typeof(Decimal) || typeof(T) == typeof(Decimal?))
            { Dynamic_DefaultValue = Convert.ToDecimal(0); }
            else if (typeof(T) == typeof(Single) || typeof(T) == typeof(Single?))
            { Dynamic_DefaultValue = Convert.ToSingle(0); }
            else if (typeof(T) == typeof(Double) || typeof(T) == typeof(Double?))
            { Dynamic_DefaultValue = Convert.ToDouble(0); }
            else if (typeof(T) == typeof(Byte) || typeof(T) == typeof(Byte?))
            { Dynamic_DefaultValue = Convert.ToByte(0); }
            else if (typeof(T) == typeof(Byte[]))
            { Dynamic_DefaultValue = new Byte[] { }; }
            else if (typeof(T) == typeof(DateTime))
            { Dynamic_DefaultValue = DateTime.MinValue; }
            else if (typeof(T) == typeof(DateTime?))
            { Dynamic_DefaultValue = (DateTime?)null; }
            else if (typeof(T) == typeof(Boolean) || typeof(T) == typeof(Boolean?))
            { Dynamic_DefaultValue = false; }
            else if (typeof(T) == typeof(String))
            { Dynamic_DefaultValue = String.Empty; }
            else if (typeof(T) == typeof(Guid))
            { Dynamic_DefaultValue = Guid.Empty; }
            else
            {
                Dynamic_DefaultValue = default(T);
            }

            return Convert_Value<T>(Value, Dynamic_DefaultValue);
        }

        /// <summary>
        /// Converts an object to a specified data type.
        /// If conversion fails, returns the specified default value.
        /// </summary>
        /// <typeparam name="T">
        /// Accepts String, Int16, Int32, Int64, Decimal, Float, Single, Double, DateTime, Boolean, Byte.
        /// </typeparam>
        /// <param name="Value">
        /// The value to be converted.
        /// </param>
        /// <param name="DefaultValue">
        /// The value to be used if the conversion fails.
        /// </param>
        /// <returns>
        /// Returns the converted value according to specified type.
        /// </returns>
        public static T Convert_Value<T>(Object Value, Object DefaultValue)
        {
            T ReturnValue = default(T);
            String ValueString = Convert_String(Value);
            Str_ParseResult Pr = new Str_ParseResult();

            if (typeof(T) == typeof(Int16) || typeof(T) == typeof(Int16?))
            { Pr = Convert_Value_ParseInt16(ValueString); }
            else if (typeof(T) == typeof(Int32) || typeof(T) == typeof(Int32?))
            { Pr = Convert_Value_ParseInt32(ValueString); }
            else if (typeof(T) == typeof(Int64) || typeof(T) == typeof(Int64?))
            { Pr = Convert_Value_ParseInt64(ValueString); }
            else if (typeof(T) == typeof(Decimal) || typeof(T) == typeof(Decimal?))
            { Pr = Convert_Value_ParseDecimal(ValueString); }
            else if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
            { Pr = Convert_Value_ParseFloat(ValueString); }
            else if (typeof(T) == typeof(Double) || typeof(T) == typeof(Double?))
            { Pr = Convert_Value_ParseDouble(ValueString); }
            else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            { Pr = Convert_Value_ParseDateTime(ValueString); }
            else if (typeof(T) == typeof(Boolean) || typeof(T) == typeof(Boolean?))
            { Pr = Convert_Value_ParseBoolean(ValueString); }
            else if (typeof(T) == typeof(Byte) || typeof(T) == typeof(Byte?))
            { Pr = Convert_Value_ParseByte(ValueString); }
            else if (typeof(T) == typeof(Byte[]))
            { Pr = Convert_Value_ParseByteArray(Value); }
            else if (typeof(T) == typeof(String))
            {
                dynamic Dynamic_DefaultValue = DefaultValue;
                Pr = new Str_ParseResult() { IsParsed = true, ParsedValue = Convert_String(Value, Dynamic_DefaultValue) };
            }
            else if (typeof(T) == typeof(Guid))
            {
                Guid OutValue;
                Pr.IsParsed = Guid.TryParse(ValueString, out OutValue);
                Pr.ParsedValue = OutValue;
            }
            else
            {
                throw new Exception("Type is not included in the specified types."); 
            }

            if (Pr.IsParsed)
            { ReturnValue = (T)Pr.ParsedValue; }
            else
            { ReturnValue = (T)DefaultValue; }

            return ReturnValue;
        }

        struct Str_ParseResult
        {
            public Boolean IsParsed;
            public Object ParsedValue;
        }

        static Str_ParseResult Convert_Value_ParseInt16(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            Int16 OutValue;
            Pr.IsParsed = Int16.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseInt32(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            Int32 OutValue;
            Pr.IsParsed = Int32.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseInt64(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            Int64 OutValue;
            Pr.IsParsed = Int64.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseDecimal(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            Decimal OutValue;
            Pr.IsParsed = Decimal.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseDouble(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            Double OutValue;
            Pr.IsParsed = Double.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseFloat(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            float OutValue;
            Pr.IsParsed = float.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseDateTime(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            DateTime OutValue;
            Pr.IsParsed = DateTime.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseBoolean(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            Boolean ReturnValue;
            if (!Boolean.TryParse(ValueString, out ReturnValue))
            {
                Pr.IsParsed = true;

                switch (ValueString.ToLower())
                {
                    case "true":
                    case "yes":
                    case "1":
                        {
                            Pr.ParsedValue = true;
                            break;
                        }
                    case "false":
                    case "no":
                    case "0":
                        {
                            Pr.ParsedValue = false;
                            break;
                        }
                    default:
                        {
                            Pr.IsParsed = false;
                            break;
                        }
                }
            }
            else
            {
                Pr.IsParsed = true;
                Pr.ParsedValue = ReturnValue;
            }

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseByte(String ValueString)
        {
            Str_ParseResult Pr = new Str_ParseResult();

            Byte OutValue;
            Pr.IsParsed = Byte.TryParse(ValueString, out OutValue);
            Pr.ParsedValue = OutValue;

            return Pr;
        }

        static Str_ParseResult Convert_Value_ParseByteArray(Object Value)
        {
            Str_ParseResult Pr = new Str_ParseResult();
            Pr.IsParsed = (Value is Byte[]);
            Pr.ParsedValue = Pr.IsParsed ? Value : new Byte[] { };

            return Pr;
        }

        public static Boolean HasValue(Object Value)
        {
            if (Value == null)
            { return false; }

            Type Obj_Type = Value.GetType();
            if (
                Obj_Type == typeof(Int16)
                || Obj_Type == typeof(Int16?)
                || Obj_Type == typeof(Int32)
                || Obj_Type == typeof(Int32?)
                || Obj_Type == typeof(Int64)
                || Obj_Type == typeof(Int64?)
                || Obj_Type == typeof(Decimal)
                || Obj_Type == typeof(Decimal?)
                || Obj_Type == typeof(Single)
                || Obj_Type == typeof(Single?)
                || Obj_Type == typeof(Double)
                || Obj_Type == typeof(Double?)
                )
            {
                if (!Value.Equals(0))
                { return true; }
            }
            else if (Obj_Type == typeof(Byte?))
            {
                if ((Value as Byte?).HasValue)
                { return true; }
            }
            else if (Obj_Type == typeof(String))
            {
                if (!String.IsNullOrEmpty((String)Value))
                { return true; }
            }
            else if (Obj_Type == typeof(DateTime))
            {
                return true;
            }
            else if (Obj_Type == typeof(DateTime?))
            {
                if ((Value as DateTime?).HasValue)
                { return true; }
            }
            else
            { return false; }

            return false;
        }

        //Code Source: https://stackoverflow.com/questions/108104/how-do-i-convert-a-system-type-to-its-nullable-version
        public static Type GetNullableType(Type type)
        {
            // Use Nullable.GetUnderlyingType() to remove the Nullable<T> wrapper if type is already nullable.
            type = Nullable.GetUnderlyingType(type) ?? type; // avoid type becoming null
            if (type.IsValueType)
                return typeof(Nullable<>).MakeGenericType(type);
            else
                return type;
        }

        public static DateTime AppendDatePart(DateTime Source, DateTime ToAppend)
        { return new DateTime(ToAppend.Year, ToAppend.Month, ToAppend.Day, Source.Hour, Source.Minute, Source.Second, Source.Millisecond); }

        public static DateTime AppendTimePart(DateTime Source, DateTime ToAppend)
        { return new DateTime(Source.Year, Source.Month, Source.Day, ToAppend.Hour, ToAppend.Minute, ToAppend.Second, ToAppend.Millisecond); }

        public static DateTime StripDatePart(DateTime Source)
        { return AppendDatePart(Source, DateTime.MinValue); }

        public static DateTime StripTimePart(DateTime Source)
        { return AppendTimePart(Source, new DateTime(1, 1, 1, 0, 0, 0)); }

        public static string SetFolderPath(string Path)
        {
            Path = Path.Trim();
            if (Path.Substring(Path.Length - 1, 1) != @"\")
            { Path += @"\"; }
            return Path;
        }

        /// <summary>
        /// DirkFF 04252013 : Function to check if certain dates are within coverage dates (overlapping)
        ///                   - either start and end is within coverage dates
        ///                   - dates to validate is totally within the coverage dates
        /// </summary>
        /// <param name="startToValidate">Entity Start date</param>
        /// <param name="endToValidate">Entity End date</param>
        /// <param name="coverageStartDate">Implem Start Date</param>
        /// <param name="coverageEndDate">Implem End Date</param>
        /// <returns></returns>
        public static bool IsOverlappingDates(DateTime startToValidate, DateTime endToValidate, DateTime coverageStartDate, DateTime coverageEndDate)
        {
            if (coverageStartDate.CompareTo(new DateTime(1900, 1, 1, 0, 0, 0)) < 0)
            {
                coverageStartDate = coverageStartDate.AddDays(1);
                coverageEndDate = coverageEndDate.AddDays(1);
            }
            if (coverageStartDate <= startToValidate && startToValidate <= coverageEndDate)
                return true;  // either start Date to validate is within coverage dates
            if (coverageStartDate <= endToValidate && endToValidate <= coverageEndDate)
                return true;  // either end Date to validate is within coverage dates
            if (startToValidate <= coverageStartDate && coverageEndDate <= endToValidate)
                return true;  // coverage dates is totally withing the dates to validate
            return false;
        }

        //Code lifted from:
        // http://stackoverflow.com/questions/12728356/how-to-get-a-base-type-of-a-ienumerable
        public static Type GetBaseTypeOfEnumerable(IEnumerable enumerable)
        {
            if (enumerable == null)
            { return null; }

            var genericEnumerableInterface = enumerable
                .GetType()
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (genericEnumerableInterface == null)
            { return null; }

            var elementType = genericEnumerableInterface.GetGenericArguments()[0];
            return elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? elementType.GetGenericArguments()[0]
                : elementType;
        }

        public static void StartLog(string functionName)
        {
            debugStart = DateTime.Now;
            Debugger.Log(0, "", String.Format("Start {0} on {1:h:mm:ss\\.ff} \n", functionName, debugStart));
        }

        public static DateTime StartLog(string functionName, DateTime passedStartTime)
        {
            passedStartTime = DateTime.Now;
            Debugger.Log(0, "", String.Format("Start {0} on {1:hh:mm:ss\\.ff} \n", functionName, passedStartTime));
            return passedStartTime;
        }

        public static void EndLog(string functionName)
        {
            debugEnd = DateTime.Now;
            Debugger.Log(0, "",
                String.Format("End {0} on {1:hh:mm:ss\\.ff} ({2:dd} day(s) {2:hh} hour(s) {2:mm} min(s) {2:ss\\.ff} sec(s)) \n",
                functionName,
                debugEnd,
                debugEnd.Subtract(debugStart)
                ));
        }

        public static void EndLog(string functionName, int noOfRecords)
        {
            debugEnd = DateTime.Now;
            Debugger.Log(0, "",
                 String.Format("End {0} on {1:hh:mm:ss\\.ff} ({2:dd} day(s) {2:hh} hour(s) {2:mm} min(s) {2:ss\\.ff} sec(s)) for {3} records. \n",
                 functionName,
                 debugEnd,
                 debugEnd.Subtract(debugStart),
                 noOfRecords
                 ));
        }

        public static void EndLog(string functionName, DateTime passedStartDate, DateTime passedEndDate)
        {
            passedEndDate = DateTime.Now;
            Debugger.Log(0, "",
                String.Format("End {0} on {1:hh:mm:ss\\.ff} ({2:dd} day(s) {2:hh} hour(s) {2:mm} min(s) {2:ss\\.ff} sec(s)) \n",
                functionName,
                passedEndDate,
                passedEndDate.Subtract(passedStartDate)
                ));
        }

        public static void EndLog(string functionName, int noOfRecords, DateTime passedStartDate, DateTime passedEndDate)
        {
            passedEndDate = DateTime.Now;
            Debugger.Log(0, "",
                 String.Format("End {0} on {1:hh:mm:ss\\.ff} ({2:dd} day(s) {2:hh} hour(s) {2:mm} min(s) {2:ss\\.ff} sec(s)) for {3} records. \n",
                 functionName,
                 passedEndDate,
                passedEndDate.Subtract(passedStartDate),
                 noOfRecords
                 ));
        }

        public static void Debug_CheckElapsed(String Label, Delegate Ds_Function, Object[] Params)
        {
            var Debug_TimeStart = DateTime.Now;
            Debug.WriteLine(String.Format("{0} Started: {1}", Label, Debug_TimeStart.ToString()));

            Ds_Function.DynamicInvoke(Params);

            var Debug_TimeEnd = DateTime.Now;
            var Debug_TimeElapsed = (Debug_TimeEnd - Debug_TimeStart);

            Debug.WriteLine(String.Format("{0} Ended: {1} ({2})", Label, Debug_TimeEnd.ToString(), Debug_TimeElapsed.ToString()));
        }

        public static string TextFiller(String TextInput, String Filler, Int32 TextLength)
        {
            string Rv = Strings.Right(Strings.StrDup(TextLength, Filler) + Strings.LTrim(Strings.Left(TextInput, TextLength)), TextLength);
            return Rv;
        }

        public static List<List<long>> DivideParamIDs(List<long> ids, int setCount)
        {
            var skipCount = 0;
            var setOfIDs = new List<List<long>>();
            while (skipCount < ids.Count)
            {
                var remainingItems = ids.Count - skipCount;
                if (setCount > remainingItems)
                    setOfIDs.Add(ids.Skip(skipCount).Take(remainingItems).ToList());
                else
                    setOfIDs.Add(ids.Skip(skipCount).Take(setCount).ToList());
                skipCount = skipCount + setCount;
            }
            return setOfIDs;
        }

        /// <summary>
        /// Returns a IEnumerable DateTime from a Date Range
        /// </summary>
        /// <param name="from">Date to start</param>
        /// <param name="thru">Date to end</param>
        /// <param name="dayInterval">Day interval count. Default is 1.</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru, Int32 dayInterval = 1)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(dayInterval))
            { yield return day; }
        }

        public static IEnumerable<DateTime> EachDayWithDays(
            DateTime from
            , DateTime thru
            , Boolean IsMon
            , Boolean IsTue
            , Boolean IsWed
            , Boolean IsThu
            , Boolean IsFri
            , Boolean IsSat
            , Boolean IsSun)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                Boolean IsValid = false;
                if (IsMon && day.DayOfWeek == DayOfWeek.Monday)
                { IsValid = true; }
                else if (IsTue && day.DayOfWeek == DayOfWeek.Tuesday)
                { IsValid = true; }
                else if (IsWed && day.DayOfWeek == DayOfWeek.Wednesday)
                { IsValid = true; }
                else if (IsThu && day.DayOfWeek == DayOfWeek.Thursday)
                { IsValid = true; }
                else if (IsFri && day.DayOfWeek == DayOfWeek.Friday)
                { IsValid = true; }
                else if (IsSat && day.DayOfWeek == DayOfWeek.Saturday)
                { IsValid = true; }
                else if (IsSun && day.DayOfWeek == DayOfWeek.Sunday)
                { IsValid = true; }

                if (IsValid)
                { yield return day; }
            }
        }

        /// <summary>
        /// Returns a IEnumerable DateTime from a Date Range, months at a time
        /// </summary>
        /// <param name="from">Date to start</param>
        /// <param name="thru">Date to end</param>
        /// <param name="dayInterval">Month interval count. Default is 1.</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> EachMonth(DateTime from, DateTime thru, Int32 monthInterval = 1)
        {
            for (var month = from.Date; month.Date <= thru.Date || month.Month == thru.Month; month = month.AddMonths(monthInterval))
            { yield return month; }
        }

        public static System.Reflection.Assembly GetAssembly()
        {
            System.Reflection.Assembly Asm = System.Reflection.Assembly.GetExecutingAssembly();
            return Asm;
        }

        public static Decimal TruncateDecimal(Decimal Input, Int32 DecimalPlaces)
        {
            Decimal Mod = 0;
            if (DecimalPlaces == 0)
            { Mod = 1; }
            else
            {
                String Mod_Zeroes = "";
                if (DecimalPlaces > 1)
                { Mod_Zeroes = Strings.StrDup(DecimalPlaces - 1, '0'); }

                Mod = Convert.ToDecimal(String.Format("0.{0}1", Mod_Zeroes));
            }

            return Input - (Input % Mod);
        }

        public static String Get_ExceptionMessage(Exception Ex)
        {
            StringBuilder Sb_Log = new StringBuilder();
            Get_ExceptionMessage(Sb_Log, Ex);
            return Sb_Log.ToString();
        }

        static void Get_ExceptionMessage(StringBuilder Sb, Exception Ex)
        {
            String LogText = "Source: {0}\r\nMessage: {1}\r\nStack Trace: {2}";
            LogText =
                String.Format(
                    LogText
                    , Ex.Source
                    , Ex.Message
                    , Ex.StackTrace);

            Sb.AppendLine(LogText);

            if (Ex.InnerException != null)
            {
                Sb.AppendLine("--Inner Exception--");
                Get_ExceptionMessage(Sb, Ex.InnerException);
            }
        }

        public static String Get_ExceptionMessage(SimpleException Ex)
        {
            StringBuilder Sb_Log = new StringBuilder();
            Get_ExceptionMessage(Sb_Log, Ex);
            return Sb_Log.ToString();
        }

        static void Get_ExceptionMessage(StringBuilder Sb, SimpleException Ex)
        {
            String LogText = $"Message: {Ex.Message}\r\nStack Trace: {Ex.StackTrace}";
            Sb.AppendLine(LogText);

            if (Ex.InnerException != null)
            {
                Sb.AppendLine("--Inner Exception--");
                Get_ExceptionMessage(Sb, Ex.InnerException);
            }
        }

        public static void Log_Exception(Exception Ex, String LogPath)
        {
            StringBuilder Sb_Log = new StringBuilder();
            Get_ExceptionMessage(Sb_Log, Ex);
            Write_Log("Exception", Sb_Log.ToString(), LogPath);
        }

        public static void Write_Log(String LogTitle, String LogText, String LogPath)
        {
            lock (Common_SyncLock)
            {
                //Get Current Date
                DateTime CurrentDate = DateTime.Now;

                FileInfo LogFile =
                    new FileInfo(
                        Path.Combine(
                            LogPath
                            , String.Format("Log_{0}.log", CurrentDate.Date.ToString("yyyyMMdd"))));

                //Create the target directory if it doesn't exists yet
                if (!LogFile.Directory.Exists)
                { LogFile.Directory.Create(); }

                LogText =
                    String.Format(
                        "[{0}] {1}\r\n{2}\r\n"
                        , LogTitle
                        , CurrentDate.ToString("yyyy/MM/dd HH:mm:ss")
                        , LogText);

                if (IsFileClosed(LogFile.FullName, true))
                {
                    //Write the LogText to File
                    StreamWriter Writer = new StreamWriter(LogFile.FullName, true);
                    Writer.AutoFlush = true;
                    Writer.WriteLine(LogText);
                    Writer.Close();
                }
            }
        }

        public static Boolean IsFileClosed(
            string filepath
            , bool wait
            , Int32 retries = 20
            , int delay = 500)
        {
            bool fileClosed = false;

            if (!File.Exists(filepath))
            { return true; }

            do
            {
                try
                {
                    // Attempts to open then close the file in RW mode, denying other users to place any locks.
                    FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.Close();
                    fileClosed = true; // success
                }
                catch (IOException) { }

                if (!wait)
                    break;

                retries--;

                if (!fileClosed)
                { Thread.Sleep(delay); }
            }
            while (!fileClosed && retries > 0);

            return fileClosed;
        }

        public static String GetSimpleExtension(String fileName)
        {
            return Path.GetExtension(fileName).Replace(".", "");
        }

        public static String RelativePath(String absPath, String relTo)
        {
            string[] absDirs = absPath.Split('\\');
            string[] relDirs = relTo.Split('\\');

            // Get the shortest of the two paths
            int len = absDirs.Length < relDirs.Length ? absDirs.Length :
            relDirs.Length;

            // Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            // Find common root
            for (index = 0; index < len; index++)
            {
                if (absDirs[index] == relDirs[index])
                    lastCommonRoot = index;
                else
                    break;
            }

            // If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
            {
                throw new ArgumentException("Paths do not have a common base");
            }

            // Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            // Add on the ..
            for (index = lastCommonRoot + 1; index < absDirs.Length; index++)
            {
                if (absDirs[index].Length > 0)
                    relativePath.Append("..\\");
            }

            // Add on the folders
            for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++)
            {
                relativePath.Append(relDirs[index] + "\\");
            }
            relativePath.Append(relDirs[relDirs.Length - 1]);

            return relativePath.ToString();
        }

        public static String GetOrdinal(Int32 value)
        {
            // Start with the most common extension.
            string extension = "th";

            // Examine the last 2 digits.
            int last_digits = value % 100;

            // If the last digits are 11, 12, or 13, use th. Otherwise:
            if (last_digits < 11 || last_digits > 13)
            {
                // Check the last digit.
                switch (last_digits % 10)
                {
                    case 1:
                        extension = "st";
                        break;
                    case 2:
                        extension = "nd";
                        break;
                    case 3:
                        extension = "rd";
                        break;
                }
            }

            return $"{value}{extension}";
        }

        public static String CleanString(String Input)
        {
            return Input.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
