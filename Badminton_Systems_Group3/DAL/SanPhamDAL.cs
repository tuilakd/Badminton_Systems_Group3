using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Badminton_Systems_Group3.DAL
{
    internal class SanPhamDAL
    {
        DatabaseHelper helper = new DatabaseHelper();

        // Lấy danh sách để hiện lên DataGrid
        public DataTable GetAll()
        {
            return helper.GetData("SELECT * FROM nhapkho");
        }

        // Kiểm tra tồn tại
        public bool CheckExists(string maSP)
        {
            DataTable dt = helper.GetData($"SELECT * FROM nhapkho WHERE MaSP = '{maSP}'");
            return dt.Rows.Count > 0;
        }

        // Thêm mới
        public bool Insert(ProductDTO dto)
        {
            string query = $"INSERT INTO nhapkho VALUES ('{dto.MaSP}', {dto.DonGia}, {dto.SoLuongTon}, '{dto.NgayNhap:yyyy-MM-dd}')";
            return helper.ExecuteNonQuery(query);
        }

        // Cập nhật (Cộng dồn số lượng theo đặc tả Luồng 3)
        public bool Update(ProductDTO dto)
        {
            string query = $"UPDATE nhapkho SET SoLuongTon = SoLuongTon + {dto.SoLuongTon}, DonGia = {dto.DonGia} WHERE MaSP = '{dto.MaSP}'";
            return helper.ExecuteNonQuery(query);
        }
    }
}