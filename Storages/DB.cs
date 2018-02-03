//╮╰╮╮▕╲╰╮╭╯╱▏╭╭╭╭
//╰╰╮╰╭╱▔▔▔▔╲╮╯╭╯
//┏━┓┏┫╭▅╲╱▅╮┣┓╭║║║
//╰┳╯╰┫┗━╭╮━┛┣╯╯╚╬╝
//╭┻╮╱╰╮╰━━╯╭╯╲┊ ║
//╰┳┫▔╲╰┳━━┳╯╱▔┊ ║
//┈┃╰━━╲▕╲╱▏╱━━━┬╨╮
//┈╰━━╮┊▕╱╲▏┊╭━━┴╥╯
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
    public class DB
    {
        public MySqlConnection connection = null;

        public DB()
        {
            string connectionString;
            connectionString = "SERVER=" +
                Configuration.mysqlServer +
                ";" +
                "DATABASE=" +
                Configuration.mysqlDatabase +
                ";" +
                "UID=" +
                Configuration.mysqlUid +
                ";" +
                "PASSWORD=" +
                Configuration.mysqlPassword +
                ";";

            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                log($"mysql: ServerVersion={connection.ServerVersion} idThread={connection.ServerThread} Database={connection.Database} State={connection.State}");
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

        public object GetSetting(string key)
        {
            string myInsertQuery = $"SELECT value FROM settings WHERE name = '{key}'";
            string value = "";

            using (var command = new MySqlCommand(myInsertQuery, connection))
            {
                using (MySqlDataReader dataReader = command.ExecuteReader())
                {
                    try
                    {
                        command.Connection = connection;
                        dataReader.Read();
                        value = (string) dataReader["value"];
                    }
                    catch (Exception e)
                    {
                        error($"db exception: {e.Message}");
                        return null;
                    }
                }
            }

            return (string) value;
        }

        public void SetSetting(string key, object value)
        {
            string myInsertQuery = $"INSERT INTO settings SET name = '{key}', value = '{value}' " +
                $"ON DUPLICATE KEY UPDATE value = '{value}'";

            try
            {
                var command = new MySqlCommand(myInsertQuery, connection);
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                error($"db exception: {e.Message}");
            }
        }
    }
}
