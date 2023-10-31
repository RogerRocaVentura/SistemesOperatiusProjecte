using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ChessServer
{
    public class DatabaseHelper
    {
        private string connectionString;

        public DatabaseHelper(string host, string database, string username, string password)
        {
            connectionString = $"Server={host};Database={database};User ID={username};Password={password};";
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
