using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using MySql.Data.MySqlClient;

namespace RPACProductionPlanner.Services
{
    public class DatabaseService
    {
        private string mssqlConn = ConfigurationManager.ConnectionStrings["MSSQLServer"].ConnectionString;
        private string mysqlConn = ConfigurationManager.ConnectionStrings["MySQLServer"].ConnectionString;
        private string sqliteConn = ConfigurationManager.ConnectionStrings["SQLiteFallback"].ConnectionString;

        // --- MSSQL with Windows Auth ---
        public DataTable ExecuteMSSQL(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(mssqlConn))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    if (parameters != null)
                    {
                        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Key, p.Value);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine("MSSQL Error: " + ex.Message);
                return null;
            }
        }

        // --- MySQL Production Tracking ---
        public DataTable ExecuteMySQL(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(mysqlConn))
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (parameters != null)
                    {
                        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Key, p.Value);
                    }
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MySQL Error: " + ex.Message);
                return null;
            }
        }

        // --- SQLite Fallback ---
        public DataTable ExecuteSQLite(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(sqliteConn))
                {
                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    if (parameters != null)
                    {
                        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Key, p.Value);
                    }
                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    da.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SQLite Error: " + ex.Message);
                return dt;
            }
        }
    }
}
