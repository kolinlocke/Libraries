//Copyright © 2012 FITS
//-----------------------------
//Author:  Stephen Hila
//Date Created: 05/14/2014
//Description: Helper class for various DB mapping.
//Revision History:
//DATE[MM/DD/YYYY]	BY[Username]		DESCRIPTION 
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data.OleDb;
using System.ComponentModel;

namespace Commons
{
    public class DatabaseMapping
    {
        // function that set the given object from the given data row
		private static void SetItemFromDataRow<T>(T item, DataRow row)
			where T : new()
		{
			// go through each column
			foreach (DataColumn c in row.Table.Columns)
			{
				// find the property for the column
				PropertyInfo p = item.GetType().GetProperty(c.ColumnName);
		 
				// if exists, set the value
				if (p != null && row[c] != DBNull.Value && p.CanWrite)
				{
                    if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    { p.SetValue(item, Convert.ChangeType(row[c], Nullable.GetUnderlyingType(p.PropertyType)), null); }
                    else
                    { p.SetValue(item, Convert.ChangeType(row[c], p.PropertyType), null); }
				}
			}
		}

        // function that creates an object from the given data row
        private static T CreateItemFromDataRow<T>(DataRow row)
            where T : new()
        {
            // create a new object
            T item = new T();
            // set the item
            SetItemFromDataRow(item, row);
 
            // return
            return item;
        }

        /// <summary>
        /// Creates a list of specified object type from a DataTable.
        /// DataTable column names should match object's property names.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public static List<T> CreateListFromDataTable<T>(DataTable tbl)
            where T : new()
        {
            // define return list
            List<T> lst = new List<T>();
            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(CreateItemFromDataRow<T>(r));
            }
            // return the list
            return lst;
        }

        /// <summary>
        /// Creates a list of specified object type from a OleDbDataReader.
        /// DataTable column names should match object's property names.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">OleDbDataReader</param>
        /// <returns></returns>
        public static List<T> CreateListFromOleDbDataReader<T>(OleDbDataReader reader)
            where T : new()
        {
            // define return list
            List<T> lst = new List<T>();
            // go through each row
            while (reader.Read())
            {
                var item = new T();

                // go through each column
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    // if exists, set the value
                    if (reader[i] != DBNull.Value)
                    {
                        // find the property for the column
                        PropertyInfo p = item.GetType().GetProperty(reader.GetName(i));

                        if (p != null)
                        {
                            if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                p.SetValue(item, Convert.ChangeType(reader[i], Nullable.GetUnderlyingType(p.PropertyType)), null);
                            else
                                p.SetValue(item, Convert.ChangeType(reader[i], p.PropertyType), null);
                        }
                    }
                }
                // add to the list
                lst.Add(item);
            }
            // return the list
            return lst;
        }

        /// <summary>
        /// Convert a IList<T> to a DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ConvertToDataTable<T>(IList<T> list) where T : class
        {
            try
            {
                DataTable table = CreateDataTable<T>();
                Type objType = typeof(T);
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(objType);
                foreach (T item in list)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor property in properties)
                    {
                        if (!CanUseType(property.PropertyType)) continue;
                        row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                    }

                    table.Rows.Add(row);
                }
                return table;
            }
            catch (DataException)
            { return null; }
            catch (Exception)
            { return null; }


        }

        public static DataRow ConvertToDataRow<T>(T Object, DataTable Table = null) where T : class
        {
            if (Table == null)
            { Table = CreateDataTable<T>(); }

            Type objType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(objType);

            DataRow row = Table.NewRow();
            foreach (PropertyDescriptor property in properties)
            {
                if (!CanUseType(property.PropertyType)) continue;
                row[property.Name] = property.GetValue(Object) ?? DBNull.Value;
            }

            return row;
        }

        public static DataTable CreateDataTable<T>() where T : class
        {
            //Type objType = typeof(T);
            //DataTable table = new DataTable(objType.Name);
            //PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(objType);
            //foreach (PropertyDescriptor property in properties)
            //{
            //    Type propertyType = property.PropertyType;
            //    if (!CanUseType(propertyType)) continue;

            //    //nullables must use underlying types
            //    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //        propertyType = Nullable.GetUnderlyingType(propertyType);
            //    //enums also need special treatment
            //    if (propertyType.IsEnum)
            //        propertyType = Enum.GetUnderlyingType(propertyType);
            //    table.Columns.Add(property.Name, propertyType);
            //}
            //return table;

            return CreateDataTable(typeof(T));
        }

        public static DataTable CreateDataTable(Type Type)
        {
            Type objType = Type;
            DataTable table = new DataTable(objType.Name);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(objType);
            foreach (PropertyDescriptor property in properties)
            {
                Type propertyType = property.PropertyType;
                if (!CanUseType(propertyType)) continue;

                //nullables must use underlying types
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                //enums also need special treatment
                if (propertyType.IsEnum)
                    propertyType = Enum.GetUnderlyingType(propertyType);
                table.Columns.Add(property.Name, propertyType);
            }
            return table;
        }

        static bool CanUseType(Type propertyType)
        {
            //only strings and value types
            if (propertyType.IsArray) return false;
            if (!propertyType.IsValueType && propertyType != typeof(string)) return false;
            return true;
        }
    }
}
