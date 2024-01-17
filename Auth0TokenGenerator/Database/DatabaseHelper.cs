using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Auth0TokenGenerator.Database
{
    public class DatabaseHelper
    {
        private static readonly string _config = ConfigurationManager.AppSettings["build"];
        //private static string connectionString = @"Data Source=.\Database\APITESTData.db;Version=3;";
        private static string connectionString = @"Data Source=APITESTData.db";

        public static void InitializeDatabase()
        {
            var connectionPath = @"APITESTData.db";
            try
            {
                //SQLiteConnection.CreateFile(connectionPath);
                using (var connection = new SQLiteConnection(connectionString))
                {

                    connection.Open();

                    // create tables
                    string createSessionsQuery = @"
                        CREATE TABLE IF NOT EXISTS Sessions ( 
                              IdToken BLOB PRIMARY KEY, 
                              AccessToken BLOB,
                              DateCreated datetime default current_timestamp, 
                              CreatedBy TEXT);";

                    // SessionToken, DateCreated, IsTokenValid, CreatedBy, CwsUrl, SchemaAlias
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = createSessionsQuery;
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during Database instantiating\nexception msg: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public static SQLiteConnection openConnection()
        {
            SQLiteConnection conn = null;
            try
            {
                SQLiteConnection connection = new SQLiteConnection(connectionString);
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    return conn = connection;
                }
                else
                    throw new Exception($"Failure to Open a connection, see exception detail ");
            }
            catch (Exception e)
            {
                MessageBox.Show("Failure to open a sql connection, try again.", "Open Connection", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return conn;
        }

        public static void CloseConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Close();
        }

        public static SQLiteDataReader SelectRecords(string sqlQuery)
        {
            SQLiteConnection conn = openConnection();
            SQLiteDataReader sqlData;
            try
            {
                var sqlCommand = new SQLiteCommand(sqlQuery, conn);
                sqlData = sqlCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while reading from the databas, see exception details here: {e.Message}");
            }

            return sqlData;
        }

        public static bool WriteDataIntoSession(string idToken, string accessToken, DateTime dateCreated, string ermailAddress)
        {
            bool returnCode = false;
            try
            {
                SQLiteConnection conn = openConnection();
                SQLiteCommand command = conn.CreateCommand();
                command.CommandType = CommandType.Text;

                command.CommandText = @"insert into Sessions(IdToken, AccessToken, DateCreated, CreatedBy) values (@idToken,@accessToken, @dateCreated, @createdBy)";

                command.Parameters.AddWithValue("@idToken", idToken);
                command.Parameters.AddWithValue("@accessToken", accessToken);
                command.Parameters.AddWithValue("@dateCreated", dateCreated);
                command.Parameters.AddWithValue("@createdBy", ermailAddress);

                command.ExecuteNonQuery();
                conn.Close();
                returnCode = true;
            }
            catch (Exception e)
            {
                throw new Exception($"Error occurred while writing to the database, {e.Message}");
            }
            return returnCode;
        }

        public static bool DeleteSession(string idToken)
        {
            bool deletedRow = false;
            try
            {
                SQLiteConnection conn = openConnection();

                using (SQLiteCommand command = new SQLiteCommand("DELETE FROM Sessions WHERE SessionToken = '" + idToken + "'", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
                deletedRow = true;
            }
            catch (SystemException ex)
            {
                MessageBox.Show($"Failure to delete the token records see exception details here: {ex}.", "Delete token", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return deletedRow;
        }

        public static bool PurgeTable()
        {
            bool deletedRow = false;
            try
            {
                SQLiteConnection conn = openConnection();

                using (SQLiteCommand command = new SQLiteCommand("DELETE FROM Sessions", conn))
                {
                    command.ExecuteNonQuery();
                }
                conn.Close();
                deletedRow = true;
            }
            catch (SystemException ex)
            {                
                MessageBox.Show($"Failure to delete all records see exception details here: {ex}.", "Purge Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return deletedRow;
        }
    }

}
