using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Data; 
using Badminton_Systems_Group3.Database; 


namespace Badminton_Systems_Group3.DAL
{
    internal class CourtDAL
    {
        private string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=QL_SanCauLong;Integrated Security=True;TrustServerCertificate=True";

        private DatabaseHelper db = new DatabaseHelper();
        public List<CourtDTO> GetAll()
        {
            List<CourtDTO> list = new List<CourtDTO>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT MaSan, TenSan, CASE WHEN TrangThai IN (N'Đã đặt', N'Trống') THEN N'Đang hoạt động' ELSE TrangThai END AS TrangThai, GiaThue FROM san"; SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new CourtDTO
                    {
                        MaSan = reader["MaSan"]?.ToString() ?? "",
                        TenSan = reader["TenSan"]?.ToString() ?? "",
                        TrangThai = reader["TrangThai"]?.ToString() ?? "Đang hoạt động",

                        GiaThue = reader["GiaThue"] != DBNull.Value ? Convert.ToDouble(reader["GiaThue"]) : 0
                    });
                }
            }
            return list;
        }
        public CourtDTO GetByMaSan(string maSan)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT MaSan, TenSan, TrangThai, GiaThue FROM san WHERE MaSan = @MaSan";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaSan", maSan);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new CourtDTO()
                    {
                        MaSan = reader["MaSan"].ToString(),
                        TenSan = reader["TenSan"].ToString(),
                        TrangThai = reader["TrangThai"].ToString(),
                        GiaThue = reader["GiaThue"] != DBNull.Value ? Convert.ToDouble(reader["GiaThue"]) : 0
                    };
                }
            }
            return null;
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

        public DataTable LayDanhSachSan(string filter = "")
        {
            string query = "SELECT MaSan, TenSan, TrangThai, GiaThue FROM san";

            if (!string.IsNullOrEmpty(filter) && filter != "Tất cả")
            {
                query += $" WHERE TrangThai = N'{filter}'";
            }

            return db.ExecuteQuery(query);
        }
    }
}
