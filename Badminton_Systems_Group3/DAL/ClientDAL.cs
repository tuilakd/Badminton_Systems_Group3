using Badminton_Systems_Group3.DTO;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace Badminton_Systems_Group3.DAL
{
    public class ClientDAL
    {
        private string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=QuanLySanCau;Integrated Security=True;TrustServerCertificate=True";
        public List<ClientDTO> GetAll()
        {
            List<ClientDTO> list = new List<ClientDTO>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT kh.MaKH, kh.HoTen, kh.SDT,
                   ds.MaDatSan, ds.TrangThai
            FROM khachhang kh
            LEFT JOIN datsan ds ON kh.MaKH = ds.MaKH
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new ClientDTO
                    {
                        MaKH = reader["MaKH"].ToString(),
                        HoTen = reader["HoTen"].ToString(),
                        SDT = reader["SDT"].ToString(),
                        MaDatSan = reader["MaDatSan"]?.ToString(),
                        TrangThai = reader["TrangThai"]?.ToString()
                    });
                }
            }
            return list;
        }
        public void Insert(ClientDTO kh)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO khachhang (MaKH, HoTen, SDT) VALUES (@MaKH, @HoTen, @SDT)";
                SqlCommand cmd = new SqlCommand(query, conn);

                MessageBox.Show("'" + kh.MaKH + "'");

                cmd.Parameters.AddWithValue("@MaKH", kh.MaKH);
                cmd.Parameters.AddWithValue("@HoTen", kh.HoTen);
                cmd.Parameters.AddWithValue("@SDT", kh.SDT);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void Delete(string maKH)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string q1 = "DELETE FROM datsan WHERE MaKH=@MaKH";
                SqlCommand cmd1 = new SqlCommand(q1, conn);
                cmd1.Parameters.AddWithValue("@MaKH", maKH);
                cmd1.ExecuteNonQuery();

                string q2 = "DELETE FROM khachhang WHERE MaKH=@MaKH";
                SqlCommand cmd2 = new SqlCommand(q2, conn);
                cmd2.Parameters.AddWithValue("@MaKH", maKH);
                cmd2.ExecuteNonQuery();
            }
        }

        public void Update(ClientDTO kh)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE khachhang 
                         SET HoTen=@HoTen, SDT=@SDT
                         WHERE MaKH=@MaKH";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@MaKH", kh.MaKH);
                cmd.Parameters.AddWithValue("@HoTen", kh.HoTen);
                cmd.Parameters.AddWithValue("@SDT", kh.SDT);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public List<ClientDTO> Search(string keyword)
        {
            List<ClientDTO> list = new List<ClientDTO>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT kh.MaKH, kh.HoTen, kh.SDT, ds.MaDatSan, ds.TrangThai
            FROM khachhang kh
            LEFT JOIN datsan ds ON kh.MaKH = ds.MaKH
            WHERE kh.MaKH LIKE @kw";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new ClientDTO
                    {
                        MaKH = reader["MaKH"].ToString(),
                        HoTen = reader["HoTen"].ToString(),
                        SDT = reader["SDT"].ToString(),
                        MaDatSan = reader["MaDatSan"]?.ToString(),
                        TrangThai = reader["TrangThai"]?.ToString()
                    });
                }
            }
            return list;
        }
        public bool CheckExist(string maKH)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM khachhang WHERE MaKH = @MaKH";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaKH", maKH);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }
    }
}