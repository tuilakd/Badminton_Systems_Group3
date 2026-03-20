using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace Badminton_Systems_Group3.Database
{
    internal class DatabaseHelper
    {
        private string connectionString =
        "Data Source=.\\SQLEXPRESS;Initial Catalog=QL_SanCL;Integrated Security=True;TrustServerCertificate=True";
        public DataTable GetData(string query)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message);
                }
            }
            return dt;
        }

        public bool ExecuteNonQuery(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Lỗi thực thi lệnh: " + ex.Message);
                    return false;
                }
            }
        }
    }
}


