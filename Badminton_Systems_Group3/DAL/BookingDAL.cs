using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System.Data.SqlClient;
using System;
using System.Data;

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
                booking.TinhThanhTien();

                string query = @"INSERT INTO datsan 
                        (MaDatSan, NgayDat, GioBD, GioKT, TrangThai, MaKH, MaSan, ThanhTien) 
                        VALUES (@mads, @ngay, @giobd, @giokt, @tt, @makh, @masan, @thanhtien)";

                SqlParameter[] parameters = {
                new SqlParameter("@mads", booking.MaDatSan),
                new SqlParameter("@ngay", booking.NgayDat.Date),
                new SqlParameter("@giobd", SqlDbType.Time) { Value = booking.GioBatDau },
                new SqlParameter("@giokt", SqlDbType.Time) { Value = booking.GioKetThuc },
                new SqlParameter("@tt", booking.TrangThai),
                new SqlParameter("@makh", booking.MaKH),
                new SqlParameter("@masan", booking.MaSan),
                new SqlParameter("@thanhtien", booking.ThanhTien)
        };

                return db.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi InsertBooking: " + ex.Message);
                return false;
            }
        }
        public DataRow GetThongTinKhachDatSanChuaThanhToan(string maSan, DateTime ngayDat)
        {
            string query = @"
                SELECT TOP 1 k.HoTen, k.SDT, d.MaDatSan, d.GioBD, d.GioKT, d.ThanhTien 
                FROM datsan d
                JOIN khachhang k ON d.MaKH = k.MaKH
                WHERE d.MaSan = @maSan 
                  AND CAST(d.NgayDat AS DATE) = @ngayDat
                  AND d.TrangThai = N'Đã đặt'
                ORDER BY d.GioBD"; 

                SqlParameter[] parameters = {
            new SqlParameter("@maSan", maSan),
            new SqlParameter("@ngayDat", ngayDat.Date)
        };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
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
                     AND TrangThai <> N'Đã hủy'";

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

            string currentMa = result.ToString(); 
            int currentNumber = int.Parse(currentMa.Substring(2));
            int nextNumber = currentNumber + 1;

            return "HD" + nextNumber.ToString("D4");
        }

        public DataRow GetThongTinKhachDatSan(string maSan, DateTime ngay, DateTime batDau, DateTime ketThuc)
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

        public bool ThanhToan(string maDatSan, string maKH, double tongTien)
        {
            string queryMax = "SELECT TOP 1 MaHD FROM hoadon ORDER BY MaHD DESC";
            object result = db.ExecuteScalar(queryMax);
            string maHDmoi = "HD0001"; 

            if (result != null && result != DBNull.Value)
            {
                int lastNum = int.Parse(result.ToString().Substring(2));
                maHDmoi = "HD" + (lastNum + 1).ToString("D4");
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

                    SqlCommand insertCmd = new SqlCommand(
                        @"INSERT INTO hoadon (MaHD, MaKH, NgayLapHD, TongTien) 
                  VALUES (@hd, @kh, @ngay, @tien)", conn, tran);
                    insertCmd.Parameters.AddWithValue("@hd", maHDmoi);
                    insertCmd.Parameters.AddWithValue("@kh", maKH);
                    insertCmd.Parameters.AddWithValue("@ngay", DateTime.Now);
                    insertCmd.Parameters.AddWithValue("@tien", tongTien);
                    insertCmd.ExecuteNonQuery();

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    System.Diagnostics.Debug.WriteLine("Lỗi: " + ex.Message);
                    return false;
                }
            }
        }
    }
}