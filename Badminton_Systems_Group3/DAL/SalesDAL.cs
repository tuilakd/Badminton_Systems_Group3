using System;
using System.Collections.Generic;
using System.Data;
using Badminton_Systems_Group3.DTO;
using Badminton_Systems_Group3.Database;
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
            // Sử dụng đúng tên cột NgayLapHD mà mình thấy trong hình của bạn
            string query = $@"INSERT INTO hoadon (MaHD, NgayLapHD, TongTien) 
                      VALUES ('{maHD}', GETDATE(), {tongTien})";
            return db.ExecuteNonQuery(query);
        }
        public bool SaveBill(List<SalesDTO> items)
        {
            return true;
        }
        public bool SaveChiTietHoaDon(string maHD, SalesDTO item)
        {
            // Câu lệnh lưu vào bảng mà bạn đang mở trong SQL Server đó
            string query = $@"INSERT INTO chitiethoadon_sp (MaHD, MaSP, SoLuong, DonGia, ThanhTien) 
                      VALUES ('{maHD}', '{item.MaSP}', {item.SoLuong}, {item.DonGia}, {item.ThanhTien})";

            return db.ExecuteNonQuery(query);
        }
        public bool CreateEmptyBill(string maHD)
        {
            // Lưu vào bảng cha trước
            string query = $"INSERT INTO hoadon (MaHD, NgayLapHD, TongTien) VALUES ('{maHD}', GETDATE(), 0)";
            return db.ExecuteNonQuery(query);
        }
        public bool UpdateStock(string maSP, int slMua)
        {
            string query = $"UPDATE SanPham SET SoLuongTon = SoLuongTon - {slMua} WHERE MaSP = '{maSP}'";
            return db.ExecuteNonQuery(query);
        }

    }

}