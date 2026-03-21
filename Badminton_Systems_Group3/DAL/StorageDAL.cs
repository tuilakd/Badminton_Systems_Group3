using System.Collections.Generic;
using System.Data;
using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
namespace Badminton_Systems_Group3.DAL
{
    public class StorageDAL
    {
        DatabaseHelper db = new DatabaseHelper();

        public DataTable LayDanhSach(List<string> filters = null)
        {
            string query = @"SELECT s.MaSP, s.TenSP, s.DonGia, s.SoLuongTon, 
                    (SELECT MAX(NgayNhap) FROM nhapkho n WHERE n.MaSP = s.MaSP) as NgayNhap 
                    FROM sanpham s";

            if (filters != null && filters.Count > 0)
            {
                string condition = "N'" + string.Join("', N'", filters) + "'";
                query += $" WHERE s.TenSP IN ({condition})";
            }

            return db.GetData(query);
        }

        public bool CapNhatTonKho(string maSP, int soLuongNhap, decimal giaMoi)
        {
            string query = string.Format(
                "UPDATE sanpham SET SoLuongTon = ISNULL(SoLuongTon, 0) + {0}, DonGia = {1} WHERE MaSP = '{2}'",
                soLuongNhap, giaMoi, maSP);

            return db.ExecuteNonQuery(query);
        }
    }
}