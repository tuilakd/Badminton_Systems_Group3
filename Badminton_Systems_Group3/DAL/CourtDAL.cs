using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Badminton_Systems_Group3.DAL
{
    internal class CourtDAL
    {
        private string connectionString = "Data Source=desktop-3453jgg\\sqlexpress;Initial Catalog=QL_SanCL;Integrated Security=True;Encrypt=False";

        public List<CourtDTO> GetAll()
        {
            List<CourtDTO> list = new List<CourtDTO>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Đã sửa 'SanBai' thành 'san'
                string query = "SELECT MaSan, TenSan, TrangThai, GiaThue FROM san";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new CourtDTO()
                    {
                        MaSan = reader["MaSan"].ToString(),
                        TenSan = reader["TenSan"].ToString(),
                        TrangThai = reader["TrangThai"].ToString(),
                        GiaThue = Convert.ToDouble(reader["GiaThue"])
                    });
                }
            }
            return list;
        }

        public bool Insert(CourtDTO sb)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO san (MaSan, TenSan, TrangThai, GiaThue) VALUES (@MaSan, @TenSan, @TrangThai, @GiaThue)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaSan", sb.MaSan);
                cmd.Parameters.AddWithValue("@TenSan", sb.TenSan);
                cmd.Parameters.AddWithValue("@TrangThai", sb.TrangThai);
                cmd.Parameters.AddWithValue("@GiaThue", sb.GiaThue);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Update(CourtDTO sb)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE san SET TenSan = @TenSan, TrangThai = @TrangThai, GiaThue = @GiaThue WHERE MaSan = @MaSan";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaSan", sb.MaSan);
                cmd.Parameters.AddWithValue("@TenSan", sb.TenSan);
                cmd.Parameters.AddWithValue("@TrangThai", sb.TrangThai);
                cmd.Parameters.AddWithValue("@GiaThue", sb.GiaThue);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool Delete(string maSan)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM san WHERE MaSan = @MaSan";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaSan", maSan);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
