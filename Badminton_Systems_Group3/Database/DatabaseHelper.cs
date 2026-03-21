using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Badminton_Systems_Group3.Database
{
    public class DatabaseHelper
    {
        // KIỂM TRA LẠI TÊN DATABASE TẠI ĐÂY (Initial Catalog)
        private string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=QuanLySanCau;Integrated Security=True;TrustServerCertificate=True";
        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
        // Hàm thực thi SELECT trả về DataTable (Dùng cho hiển thị danh sách)
        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(parameters);
                        }
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Lỗi ExecuteQuery: " + ex.Message);
                }
            }
            return dt;
        }

        // Hàm thực thi trả về 1 giá trị duy nhất (Dùng cho SELECT COUNT để check trùng lịch)
        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Lỗi ExecuteScalar: " + ex.Message);
                    return null;
                }
            }
        }

        // Hàm thực thi INSERT, UPDATE, DELETE (Dùng để lưu đặt sân)
        public bool ExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(parameters);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Nếu lỗi Invalid Column Name xuất hiện ở đây, nghĩa là query truyền vào sai tên cột
                    System.Windows.MessageBox.Show("Lỗi thực thi lệnh SQL: " + ex.Message);
                    return false;
                }
            }
        }
    }
}