using System;
using System.Data.Common;
using System.Reflection;
using System.Web;

namespace DatabaseFunctions
{
    public abstract class TableBase<T> where T : new()
    {
        protected TableBase()
        { }

        public T Create(DbDataReader reader)
        {
            T table = new T();

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (reader.HasColumn(prop.Name))
                {
                    try
                    {
                        Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;                        

                        object value = (t == typeof(int?) || t == typeof(int)) && string.IsNullOrEmpty(reader[prop.Name]?.ToString()) ? default : reader[prop.Name];
                        value = (t == typeof(DateTime) || t == typeof(DateTime?)) && string.IsNullOrEmpty(value?.ToString()) ? Convert.DBNull : value;
                        value = t == typeof(string) && !string.IsNullOrEmpty(value?.ToString()) ? HttpUtility.HtmlDecode(value.ToString())?.Trim() : value;
                        value = t == typeof(string) && (value == null || value == Convert.DBNull) ? string.Empty : value;

                        object safeValue = value == null || Convert.IsDBNull(value) ? null : Convert.ChangeType(value, t);

                        prop.SetValue(table, safeValue);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{ex.Message}.\r\nPropriété: {prop.Name}", ex);
                    }
                }
            }
            return table;
        }

    }
}
