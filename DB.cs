using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;
using static forexAI.Logger;

namespace forexAI
{
    internal class DB
    {
        private MySqlConnection connection = null;

        public DB()
        {
            string connectionString;
            connectionString = "SERVER=" + Configuration.mysql_server + ";" + "DATABASE=" + Configuration.mysql_database +
                ";" + "UID=" + Configuration.mysql_uid + ";" + "PASSWORD=" + Configuration.mysql_password + ";";

            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                debug($"opened mysql connection: version={connection.ServerVersion} id={connection.ServerThread} db={connection.Database}" +
                    $" connection={connection.GetHashCode()}");
                return;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        error($"Cannot connect to server.  Contact administrator [{ex.Message}]");
                        break;

                    case 1045:
                        error($"Invalid username/password, please try again  [{ex.Message}]");
                        break;

                    default:
                        error($"mysql error: {ex.Message}");
                        break;
                }
                return;
            }
        }

        internal void StoreSetting(string key, object value)
        {
            string myInsertQuery = $"INSERT INTO settings SET name = '{key}', value = '{value}' " +
                $"ON DUPLICATE KEY UPDATE value = '{value}'";

            var command = new MySqlCommand(myInsertQuery, connection);
            command.Connection = connection;
            command.ExecuteNonQuery();
        }

        internal object GetSetting(string key)
        {
            string myInsertQuery = $"SELECT value FROM settings WHERE name = '{key}'";
            var command = new MySqlCommand(myInsertQuery, connection);
            MySqlDataReader dataReader = command.ExecuteReader();
            command.Connection = connection;
            dataReader.Read();
            string value = (string) dataReader["value"];
            return value;
        }

        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                error(ex.Message);
                return false;
            }
        }
    }
}