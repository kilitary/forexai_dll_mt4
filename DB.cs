using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;
using static forexAI.Logger;

namespace forexAI
{
    internal static class DB
    {
        static private MySqlConnection connection = null;
        static private string server = "192.168.10.10";
        static private string database = "forex";
        static private string uid = "homestead";
        static private string password = "secret";

        public static void Init()
        {
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                debug($"opened mysql connection: version={connection.ServerVersion} id={connection.ServerThread} db={connection.Database}");
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

        static public bool CloseConnection()
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