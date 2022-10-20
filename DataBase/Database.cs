using Services.Interfaces;
using System.Data;
using System.Dynamic;
using DatabaseFunctions;
using Extensions;
using MySql.Data.MySqlClient;

namespace DataBase
{
    public static class Database
    {
        private static IGlobalService _globalService;

        public static void Configure(IGlobalService globalService)
        {
            _globalService = globalService;
        }

        public static MySqlConnection CreateConnection()
        {
#if DEBUG
            return new MySqlConnection(
                $"Server=localhost; " +
                $"Database=world; " +
                $"Uid=root; " +
                $"Pwd=H281198d.; " +
                $"Convert Zero Datetime=True;");
#else
            return new MySqlConnection(
                $"Server=db5010457625.hosting-data.io; " +
                $"Database=dbs8858506; " +
                $"Uid=dbu2077634; " +
                $"Pwd=H281198d.;");

#endif
        }

        #region Execution

        public static async Task<IEnumerable<T>> ExecuteReader<T>(string query, List<MySqlParameter> cmdParameters = null, CommandType cmdType = CommandType.Text)
            where T :  new()
        {
            List<T> items = new List<T>();

            using (var conn = CreateConnection())
            {
                conn.Open();

                using (MySqlTransaction transaction = conn.BeginTransaction())
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    PrepareCommand(cmd, query, cmdParameters, cmdType);

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (await sdr.ReadAsync())
                        {
                            if (new T() is TableBase<T> t)
                            {
                                items.Add(t.Create(sdr));
                            }
                            else
                            {
                                items.Add((T)sdr[0]);
                            }
                        }
                        sdr.Close();
                    }
                    transaction.Commit();
                    cmd.Parameters.Clear();
                }
                conn.Close();
            }
            return items;
        }

        public static async Task<T> ExecuteReaderSingleResult<T>(string query, List<MySqlParameter> cmdParameters = null, CommandType cmdType = CommandType.Text) 
            where T : TableBase<T>, new()
        {
            T item = default;

            using (var conn = CreateConnection())
            {
                conn.Open();

                using (MySqlTransaction transaction = conn.BeginTransaction())
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    PrepareCommand(cmd, query, cmdParameters, cmdType);

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        await sdr.ReadAsync();
                        if (sdr.HasRows)
                            item = new T().Create(sdr);

                        sdr.Close();
                    }
                    transaction.Commit();
                    cmd.Parameters.Clear();
                }
                conn.Close();
            }
            return item;
        }

        public static async Task<int> ExecuteNonQuery(string query, List<MySqlParameter> cmdParameters = null, CommandType cmdType = CommandType.Text)
        {
            int result = 0;

            using (var conn = CreateConnection())
            {
                conn.Open();

                using (MySqlTransaction transaction = conn.BeginTransaction())
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    PrepareCommand(cmd, query, cmdParameters, cmdType);
                    result = await cmd.ExecuteNonQueryAsync();

                    transaction.Commit();
                    cmd.Parameters.Clear();
                }
                conn.Close();
            }
            return result;
        }

        public static async Task<IEnumerable<dynamic>> ExecuteReaderDynamic(string query, List<MySqlParameter> cmdParameters = null, CommandType cmdType = CommandType.Text)
        {
            List<dynamic> items = new List<dynamic>();

            using (DataTable datatable = new DataTable())
            using (var conn = CreateConnection())
            {
                conn.Open();

                using (MySqlTransaction transaction = conn.BeginTransaction())
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    PrepareCommand(cmd, query, cmdParameters, cmdType);

                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (await sdr.ReadAsync())
                        {
                            ExpandoObject item = new ExpandoObject();
                            if (item is IDictionary<string, object> expandoDict)
                            {
                                for (int i = 0; i < sdr.FieldCount; i++)
                                {
                                    var value = sdr.GetValue(i);
                                    expandoDict[sdr.GetName(i)] = value != DBNull.Value ? value : null;
                                }
                            }
                            items.Add(item);
                        }
                        sdr.Close();
                    }
                    transaction.Commit();
                    cmd.Parameters.Clear();

                }
                conn.Close();
                return items;
            }
        }

        public static async Task<dynamic> ExecuteReaderDynamicSingleResult(string query, List<MySqlParameter> cmdParameters = null, CommandType cmdType = CommandType.Text)
        {
            ExpandoObject item = null;

            using (DataTable datatable = new DataTable())
            using (var conn = CreateConnection())
            {
                conn.Open();

                using (MySqlTransaction transaction = conn.BeginTransaction())
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    PrepareCommand(cmd, query, cmdParameters, cmdType);
                    using (MySqlDataReader sdr = cmd.ExecuteReader())
                    {
                        await sdr.ReadAsync();
                        if (sdr.HasRows)
                        {
                            item = new ExpandoObject();
                            if (item is IDictionary<string, object> expandoDict)
                            {
                                for (int i = 0; i < sdr.FieldCount; i++)
                                {
                                    var value = sdr.GetValue(i);
                                    expandoDict[sdr.GetName(i)] = value != DBNull.Value ? value : null;
                                }
                            }
                        }                           
                        sdr.Close();
                    }
                    transaction.Commit();
                    cmd.Parameters.Clear();

                }
                conn.Close();
                return item;
            }
        }

        #endregion

        #region Command Preparation

        private static void PrepareCommand(MySqlCommand cmd, string query, List<MySqlParameter> cmdParameters, CommandType cmdType)
        {
            cmd.CommandType = cmdType;
            cmd.CommandText = query;
            cmd.CommandTimeout = 600;

            if (cmdParameters != null)
                AttachParameters(cmd, cmdParameters);
        }

        private static void AttachParameters(MySqlCommand cmd, List<MySqlParameter> cmdParameters)
        {
            foreach (MySqlParameter param in cmdParameters)
            {
                if (param != null)
                {
                    param.Value = param.Value.CheckDbNull();
                    cmd.Parameters.Add(param);
                }
            }
        }

        #endregion
    }
}
