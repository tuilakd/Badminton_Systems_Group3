using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Data;
using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System.Data.SqlClient;

namespace Badminton_Systems_Group3.DAL
{
    public class SalesDAL
    {
        private DatabaseHelper db = new DatabaseHelper();

        public List<ProductDTO> GetProducts()
        {
            List<ProductDTO> list = new List<ProductDTO>();
            string query = "SELECT MaSP, TenSP, DonGia, SoLuongTon FROM SanPham";
            DataTable dt = db.GetData(query);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ProductDTO
                {
                    MaSP = row["MaSP"].ToString(),
                    TenSP = row["TenSP"].ToString(),
                    DonGia = Convert.ToDecimal(row["DonGia"]),
                    SoLuongTon = Convert.ToInt32(row["SoLuongTon"]),
                });
            }
            return list;
        }

        public bool CreateHoaDon(string maHD, decimal tongTien)
        {
            string query = "INSERT INTO hoadon (MaHD, MaKH, NgayLapHD, TongTien) VALUES (@MaHD, NULL, GETDATE(), @TongTien)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaHD", maHD),
                new SqlParameter("@TongTien", tongTien)
            };

            return db.ExecuteNonQuery(query, parameters);
        }

        public bool SaveBill(List<SalesDTO> items)
        {
            return true;
        }

        public bool SaveChiTietHoaDon(string maHD, SalesDTO item)
        {
            string query = "INSERT INTO chitiethoadon_sp (MaHD, MaSP, SoLuong, DonGia, ThanhTien) VALUES (@MaHD, @MaSP, @SoLuong, @DonGia, @ThanhTien)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaHD", maHD),
                new SqlParameter("@MaSP", item.MaSP),
                new SqlParameter("@SoLuong", item.SoLuong),
                new SqlParameter("@DonGia", item.DonGia),
                new SqlParameter("@ThanhTien", item.ThanhTien)
            };

            return db.ExecuteNonQuery(query, parameters);
        }

        public bool CreateEmptyBill(string maHD)
        {
            string query = "INSERT INTO hoadon (MaHD, MaKH, NgayLapHD, TongTien) VALUES (@MaHD, NULL, GETDATE(), 0)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaHD", maHD)
            };

            return db.ExecuteNonQuery(query, parameters);
        }

        public bool UpdateStock(string maSP, int slMua)
        {
            string query = "UPDATE SanPham SET SoLuongTon = SoLuongTon - @SlMua WHERE MaSP = @MaSP";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SlMua", slMua),
                new SqlParameter("@MaSP", maSP)
            };

            return db.ExecuteNonQuery(query, parameters);
        }
    }
}