using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Badminton_Systems_Group3.Database;

namespace Badminton_Systems_Group3.DAL
{
    public class RevenueDAL
    {
        private DatabaseHelper db = new DatabaseHelper();

        public decimal GetDoanhThu(string type)
        {
            string query = "";
            if (type == "Tong")
            {
                query = "SELECT SUM(TongTien) FROM hoadon";
            }
            else if (type == "HomNay")
            {
                query = "SELECT SUM(TongTien) FROM hoadon WHERE CAST(NgayLapHD AS DATE) = CAST(GETDATE() AS DATE)";
            }
            else if (type == "ThangNay")
            {
                query = "SELECT SUM(TongTien) FROM hoadon WHERE MONTH(NgayLapHD) = MONTH(GETDATE()) AND YEAR(NgayLapHD) = YEAR(GETDATE())";
            }

            object result = db.ExecuteScalar(query);

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDecimal(result);
            }
            return 0;
        }

        public DataTable GetDanhSachGiaoDich(string tuNgay, string denNgay, string loaiHD)
        {
            string querySP = @"
                SELECT h.MaHD, h.NgayLapHD, N'Bán hàng' AS LoaiHoaDon, 
                       sp.TenSP AS ChiTiet, ct.DonGia AS DonGia, ct.SoLuong AS SoLuong, ct.ThanhTien
                FROM hoadon h
                INNER JOIN chitiethoadon_sp ct ON h.MaHD = ct.MaHD
                INNER JOIN sanpham sp ON ct.MaSP = sp.MaSP";

            string querySan = @"
                SELECT h.MaHD, h.NgayLapHD, N'Đặt sân' AS LoaiHoaDon, 
                       s.TenSan AS ChiTiet, ct.GiaThue AS DonGia, 
                       DATEDIFF(HOUR, ds.GioBD, ds.GioKT) AS SoLuong, 
                       (ct.GiaThue * DATEDIFF(HOUR, ds.GioBD, ds.GioKT)) AS ThanhTien
                FROM hoadon h
                INNER JOIN chitiethoadon_san ct ON h.MaHD = ct.MaHD
                INNER JOIN datsan ds ON ct.MaDatSan = ds.MaDatSan
                INNER JOIN san s ON ds.MaSan = s.MaSan";

            string finalQuery = "";

            if (loaiHD == "Bán hàng") finalQuery = querySP;
            else if (loaiHD == "Đặt sân") finalQuery = querySan;
            else finalQuery = querySP + " UNION ALL " + querySan;

            string wrapperQuery = $"SELECT * FROM ({finalQuery}) AS BangGop WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(tuNgay))
            {
                wrapperQuery += " AND CAST(NgayLapHD AS DATE) >= @TuNgay";
                parameters.Add(new SqlParameter("@TuNgay", tuNgay));
            }

            if (!string.IsNullOrEmpty(denNgay))
            {
                wrapperQuery += " AND CAST(NgayLapHD AS DATE) <= @DenNgay";
                parameters.Add(new SqlParameter("@DenNgay", denNgay));
            }

            wrapperQuery += " ORDER BY NgayLapHD DESC";

            return db.ExecuteQuery(wrapperQuery, parameters.ToArray());
        }

        private string TaoMaBaoCaoTuDong()
        {
            string query = "SELECT TOP 1 MaBaoCao FROM doanhthu ORDER BY MaBaoCao DESC";
            object result = db.ExecuteScalar(query);

            if (result != null && result != DBNull.Value)
            {
                string lastMa = result.ToString();
                int so = int.Parse(lastMa.Substring(2)) + 1;
                return "BC" + so.ToString("D4");
            }
            return "BC0001";
        }

        public bool LuuBaoCaoDoanhThu(decimal tongTien)
        {
            string maBC = TaoMaBaoCaoTuDong();
            DateTime now = DateTime.Now;

            string query = "INSERT INTO doanhthu (MaBaoCao, Thang, Ngay, Nam, tongDoanhThu) VALUES (@MaBaoCao, @Thang, @Ngay, @Nam, @TongTien)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaBaoCao", maBC),
                new SqlParameter("@Thang", now.Month),
                new SqlParameter("@Ngay", now.Day),
                new SqlParameter("@Nam", now.Year),
                new SqlParameter("@TongTien", tongTien)
            };

            return db.ExecuteNonQuery(query, parameters);
        }
    }
}