using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using MySql.Data.MySqlClient;

namespace Cronical.MySql
{
    /// <summary>
    /// Minimal helper class for database queries.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Instantiate a new short-lived connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static MySqlConnection GetConnection(string connection)
        {
            var result = new MySqlConnection(connection);
            result.Open();
            return result;
        }

        /// <summary>
        /// Connection extension method to execute an SQL query with no result set.
        /// </summary>
        /// <param name="connection">Connection to use.</param>
        /// <param name="query">Query to run.</param>
        /// <returns>Rows affected.</returns>
        public static int Execute(this MySqlConnection connection, string query)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = query;
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Connection extension method to load a list of objects.
        /// </summary>
        /// <param name="connection">Connection to use.</param>
        /// <param name="query">Query to run.</param>
        /// <returns>A list of dynamic objects containing the result set.</returns>
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

        /// <summary>
        /// Connection extension method for quickly returning a single scalar value.
        /// </summary>
        /// <typeparam name="T">Type to return.</typeparam>
        /// <param name="connection">Connection to use.</param>
        /// <param name="query">Query to run.</param>
        /// <returns>The first value of the first row.</returns>
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
