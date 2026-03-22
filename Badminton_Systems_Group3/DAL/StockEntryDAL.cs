using Badminton_Systems_Group3.DTO;
using Badminton_Systems_Group3.Database;

namespace Badminton_Systems_Group3.DAL
{
    public class StockEntryDAL
    {
        DatabaseHelper db = new DatabaseHelper();

        public bool ThemPhieuNhap(StockEntryDTO entry)
        {
            string query = string.Format("INSERT INTO nhapkho (MaSP, DonGia, SoLuongNhap, NgayNhap) VALUES ('{0}', {1}, {2}, '{3}')",
                            entry.MaSP, entry.DonGia, entry.SoLuongNhap, entry.NgayNhap.ToString("yyyy-MM-dd"));
            return db.ExecuteNonQuery(query);
        }
    }
}