using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;
using MySql.Data.MySqlClient;

namespace Cronical.MySql
{
    public static class Helper
    {
        public static MySqlConnection GetConnection(string connection)
        {
            var result = new MySqlConnection(connection);
            result.Open();
            return result;
        }

        public static int Execute(this MySqlConnection connection, string query)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                return cmd.ExecuteNonQuery();
            }
        }

        public static List<dynamic> Query(this MySqlConnection connection, string query)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    var fields = new List<string>();
                    for (var i = 0; i < reader.FieldCount; i++)
                        fields.Add(reader.GetName(i));

                    var result = new List<dynamic>();
                    while (reader.Read())
                    {
                        var row = new ExpandoObject();
                        var setter = (IDictionary<string, object>) row;
                        
                        for (var i = 0; i < reader.FieldCount; i++)
                            setter[fields[i]] = reader.GetValue(i);

                        result.Add(row);
                    }

                    return result;
                }
            }
        }

        public static T QueryScalar<T>(this MySqlConnection connection, string query)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                return (T)cmd.ExecuteScalar();
            }
        }
    }
}
