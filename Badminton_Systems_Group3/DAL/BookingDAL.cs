using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System;
using System.Data;
using System.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Badminton_Systems_Group3.DAL
{
    public class BookingDAL
    {
        private readonly DatabaseHelper db = new DatabaseHelper();

        public bool InsertKhachHang(string maKH, string hoTen, string sdt)
        {
            try
            {
                string checkQuery = "SELECT COUNT(*) FROM khachhang WHERE SDT = @sdt";

                SqlParameter[] checkParams = {
                    new SqlParameter("@sdt", sdt)
                };

                int exists = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));

                if (exists > 0) return true;

                string query = @"INSERT INTO khachhang (MaKH, HoTen, SDT)
                                 VALUES (@makh, @hoten, @sdt)";

                SqlParameter[] parameters = {
                    new SqlParameter("@makh", maKH),
                    new SqlParameter("@hoten", hoTen),
                    new SqlParameter("@sdt", sdt)
                };

                return db.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi InsertKhachHang: " + ex.Message);
                return false;
            }
        }

        public bool KhachHangTonTai(string maKH)
        {
            string query = "SELECT COUNT(*) FROM khachhang WHERE MaKH = @ma";

            SqlParameter[] parameters = {
                new SqlParameter("@ma", maKH)
            };

            object result = db.ExecuteScalar(query, parameters);

            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool InsertBooking(BookingDTO booking)
        {
            try
            {
                string sqlGetGia = "SELECT GiaThue FROM san WHERE MaSan = @maSan";
                SqlParameter[] pGetGia = { new SqlParameter("@maSan", booking.MaSan) };
                object giaGoc = db.ExecuteScalar(sqlGetGia, pGetGia);

                booking.GiaThue = giaGoc != DBNull.Value ? Convert.ToDecimal(giaGoc) : 0;
                booking.TinhThanhTien(); 

                string query = @"INSERT INTO datsan 
                (MaDatSan, NgayDat, GioBD, GioKT, TrangThai, MaKH, MaSan, GiaThue, ThanhTien) 
                VALUES (@mads, @ngay, @giobd, @giokt, @tt, @makh, @masan, @giathue, @thanhtien)";

                SqlParameter[] parameters = {
                new SqlParameter("@mads", booking.MaDatSan),
                new SqlParameter("@ngay", booking.NgayDat.Date),
                new SqlParameter("@giobd", SqlDbType.Time) { Value = booking.GioBatDau },
                new SqlParameter("@giokt", SqlDbType.Time) { Value = booking.GioKetThuc },
                new SqlParameter("@tt", booking.TrangThai ?? "Đã đặt"),
                new SqlParameter("@makh", booking.MaKH),
                new SqlParameter("@masan", booking.MaSan),
                new SqlParameter("@giathue", booking.GiaThue),
                new SqlParameter("@thanhtien", booking.ThanhTien)
            };

                return db.ExecuteNonQuery(query, parameters);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public DataRow GetThongTinKhachDatSanChuaThanhToan(string maSan, DateTime ngay, TimeSpan gioBD, TimeSpan gioKT)
        {
            string query = @"
                SELECT ds.MaDatSan, kh.HoTen, kh.SDT, ds.GioBD, ds.GioKT, ds.ThanhTien
                FROM datsan ds
                INNER JOIN khachhang kh ON ds.MaKH = kh.MaKH
                WHERE ds.MaSan = @MaSan 
                  AND ds.NgayDat = @Ngay 
                  AND ds.TrangThai = N'Đã đặt'
                  AND ds.GioBD = @GioBD 
                  AND ds.GioKT = @GioKT"; 

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSan", maSan),
                new SqlParameter("@Ngay", ngay),
                new SqlParameter("@GioBD", gioBD),
                new SqlParameter("@GioKT", gioKT)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }
            return null;
        }
        public bool KiemTraTrungGio(BookingDTO booking)
        {
            string query = @"SELECT COUNT(*) FROM datsan 
                             WHERE MaSan = @masan 
                             AND CAST(NgayDat AS DATE) = @ngaydat
                             AND (@giobd < GioKT AND @giokt > GioBD)
                             AND TrangThai <> N'Đã hủy'";

                SqlParameter[] parameters = {
                new SqlParameter("@masan", booking.MaSan),
                new SqlParameter("@ngaydat", booking.NgayDat.Date),
                new SqlParameter("@giobd", SqlDbType.Time) { Value = booking.GioBatDau },
                new SqlParameter("@giokt", SqlDbType.Time) { Value = booking.GioKetThuc }
            };

            object result = db.ExecuteScalar(query, parameters);

            return result != null && Convert.ToInt32(result) > 0;
        }

        public DataTable GetSanDaDat(DateTime ngay, TimeSpan gioBD, TimeSpan gioKT)
        {
            string query = @"SELECT DISTINCT MaSan FROM datsan 
                 WHERE CAST(NgayDat AS DATE) = @ngaydat 
                 AND (@giobd < GioKT AND @giokt > GioBD) 
                 AND TrangThai = N'Đã đặt'";

            SqlParameter[] parameters = {
            new SqlParameter("@ngaydat", ngay.Date),
            new SqlParameter("@giobd", SqlDbType.Time) { Value = gioBD },
            new SqlParameter("@giokt", SqlDbType.Time) { Value = gioKT }
};

            return db.ExecuteQuery(query, parameters);
        }
        public string GetNewMaHD()
        {
            string query = "SELECT TOP 1 MaHD FROM hoadon ORDER BY MaHD DESC";
            object result = db.ExecuteScalar(query);

            if (result == null || result == DBNull.Value)
            {
                return "HD0001"; 
            }

            string currentMa = result?.ToString() ?? "HD0000";
            int currentNumber = int.Parse(currentMa.Substring(2));
            int nextNumber = currentNumber + 1;

            return "HD" + nextNumber.ToString("D4");
        }
        public DataTable SearchBooking(string trangThai, string keyword, DateTime? ngay)
        {
            string query = @"
                SELECT 
                    ds.MaDatSan,
                    s.TenSan,
                    ds.TrangThai,
                    kh.HoTen,
                    kh.SDT,
                    ds.NgayDat,
                    ds.GioBD,
                    ds.GioKT
                FROM datsan ds
                JOIN khachhang kh ON ds.MaKH = kh.MaKH
                JOIN san s ON ds.MaSan = s.MaSan
                WHERE 1=1 ";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                query += " AND ds.TrangThai = @TrangThai";
                parameters.Add(new SqlParameter("@TrangThai", trangThai));
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query += @" AND (
            kh.HoTen LIKE @kw 
            OR kh.SDT LIKE @kw
            OR s.TenSan LIKE @kw
        )";
                parameters.Add(new SqlParameter("@kw", "%" + keyword + "%"));
            }

            if (ngay.HasValue)
            {
                query += " AND CAST(ds.NgayDat AS DATE) = @Ngay";
                parameters.Add(new SqlParameter("@Ngay", ngay.Value.Date));
            }

            query += " ORDER BY ds.NgayDat DESC, ds.GioBD ASC";

            return db.ExecuteQuery(query, parameters.ToArray());
        }
        public bool HuyLich(string maDatSan)
        {
            string query = "UPDATE datsan SET TrangThai = N'Đã hủy' WHERE MaDatSan = @ma";

            SqlParameter[] parameters = {
        new SqlParameter("@ma", maDatSan)
    };

            return db.ExecuteNonQuery(query, parameters);
        }
        public bool UpdateBooking(string maDatSan, DateTime ngay, TimeSpan gioBD, TimeSpan gioKT)
        {
            string query = @"
                UPDATE datsan 
                SET NgayDat = @ngay,
                    GioBD = @bd,
                    GioKT = @kt
                WHERE MaDatSan = @ma";

            SqlParameter[] parameters = {
        new SqlParameter("@ngay", ngay.Date),
        new SqlParameter("@bd", SqlDbType.Time){ Value = gioBD },
        new SqlParameter("@kt", SqlDbType.Time){ Value = gioKT },
        new SqlParameter("@ma", maDatSan)
    };

            return db.ExecuteNonQuery(query, parameters);
        }

        public DataRow? GetThongTinKhachDatSan(string maSan, DateTime ngay, DateTime batDau, DateTime ketThuc)
        {
            string query = @"SELECT k.HoTen, k.SDT, d.MaDatSan, d.ThanhTien 
                             FROM datsan d 
                             JOIN khachhang k ON d.MaKH = k.MaKH 
                             WHERE d.MaSan = @maSan 
                             AND CAST(d.NgayDat AS DATE) = @ngay
                             AND d.GioBD = @start 
                             AND d.GioKT = @end";

            SqlParameter[] parameters = {
                new SqlParameter("@maSan", maSan),
                new SqlParameter("@ngay", ngay.Date),
                new SqlParameter("@start", SqlDbType.Time) { Value = batDau.TimeOfDay },
                new SqlParameter("@end", SqlDbType.Time) { Value = ketThuc.TimeOfDay }
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
        public DataTable GetAllBookingSchedule()
        {
            string query = @"SELECT d.MaDatSan,d.MaSan, s.TenSan, d.TrangThai, kh.HoTen, kh.SDT, 
                            d.NgayDat, d.GioBD, d.GioKT
                     FROM datsan d
                     JOIN san s ON d.MaSan = s.MaSan
                     JOIN khachhang kh ON d.MaKH = kh.MaKH
                     ORDER BY d.NgayDat DESC, d.GioBD ASC";

            return db.ExecuteQuery(query);
        }
        public bool KiemTraTrungGioUpdate(BookingDTO booking, string maDatSan)
        {
            string query = @"SELECT COUNT(*) FROM datsan 
                     WHERE MaSan = @masan 
                     AND CAST(NgayDat AS DATE) = @ngaydat
                     AND (@giobd < GioKT AND @giokt > GioBD)
                     AND TrangThai <> N'Đã hủy'
                     AND MaDatSan <> @ma"; 

            SqlParameter[] parameters = {
        new SqlParameter("@masan", booking.MaSan),
        new SqlParameter("@ngaydat", booking.NgayDat.Date),
        new SqlParameter("@giobd", SqlDbType.Time) { Value = booking.GioBatDau },
        new SqlParameter("@giokt", SqlDbType.Time) { Value = booking.GioKetThuc },
        new SqlParameter("@ma", maDatSan)
    };

            object result = db.ExecuteScalar(query, parameters);

            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool ThanhToan(string maDatSan, string maKH, double tongTien)
        {
            string queryMax = "SELECT TOP 1 MaHD FROM hoadon ORDER BY MaHD DESC";
            object resultObj = db.ExecuteScalar(queryMax);
            string maHDmoi = "HD0001";

            if (resultObj != null && resultObj != DBNull.Value)
            {
                string currentMa = resultObj.ToString();
                if (currentMa.Length >= 6)
                {
                    if (int.TryParse(currentMa.Substring(2), out int lastNum))
                    {
                        maHDmoi = "HD" + (lastNum + 1).ToString("D4");
                    }
                }
            }

            using (SqlConnection conn = db.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    SqlCommand updateCmd = new SqlCommand(
                        "UPDATE datsan SET TrangThai = N'Đã thanh toán' WHERE MaDatSan = @ma", conn, tran);
                    updateCmd.Parameters.AddWithValue("@ma", maDatSan);
                    updateCmd.ExecuteNonQuery();

                    SqlCommand insertHDCmd = new SqlCommand(
                        @"INSERT INTO hoadon (MaHD, MaKH, NgayLapHD, TongTien) 
                  VALUES (@hd, @kh, @ngay, @tien)", conn, tran);
                    insertHDCmd.Parameters.AddWithValue("@hd", maHDmoi);
                    insertHDCmd.Parameters.AddWithValue("@kh", maKH);
                    insertHDCmd.Parameters.AddWithValue("@ngay", DateTime.Now);
                    insertHDCmd.Parameters.AddWithValue("@tien", tongTien);
                    insertHDCmd.ExecuteNonQuery();

                    SqlCommand detailCmd = new SqlCommand(
                        @"INSERT INTO chitiethoadon_san (MaHD, MaDatSan, GiaThue, ThanhTien)
                  SELECT @hd, MaDatSan, 
                         (SELECT GiaThue FROM san WHERE san.MaSan = datsan.MaSan), 
                         ThanhTien
                  FROM datsan WHERE MaDatSan = @maDS", conn, tran);
                    detailCmd.Parameters.AddWithValue("@hd", maHDmoi);
                    detailCmd.Parameters.AddWithValue("@maDS", maDatSan);
                    detailCmd.ExecuteNonQuery();

                    SqlCommand updateSan = new SqlCommand(
                        @"UPDATE san SET TrangThai = N'Trống' 
                  WHERE MaSan = (SELECT MaSan FROM datsan WHERE MaDatSan = @maDS)", conn, tran);
                    updateSan.Parameters.AddWithValue("@maDS", maDatSan);
                    updateSan.ExecuteNonQuery();

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    System.Diagnostics.Debug.WriteLine("Lỗi trong quá trình thanh toán: " + ex.Message);
                    return false;
                }
            } 

        }
    }
}