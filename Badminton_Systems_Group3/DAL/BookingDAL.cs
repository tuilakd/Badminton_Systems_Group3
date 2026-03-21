using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Badminton_Systems_Group3.DAL
{
    public class BookingDAL
    {
        private readonly DatabaseHelper db = new DatabaseHelper();

        // ================= 1. LƯU KHÁCH =================
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

        // ================= 2. KIỂM TRA KHÁCH =================
        public bool KhachHangTonTai(string maKH)
        {
            string query = "SELECT COUNT(*) FROM khachhang WHERE MaKH = @ma";

            SqlParameter[] parameters = {
                new SqlParameter("@ma", maKH)
            };

            object result = db.ExecuteScalar(query, parameters);

            return result != null && Convert.ToInt32(result) > 0;
        }

        // ================= 3. INSERT ĐẶT SÂN =================
        public bool InsertBooking(BookingDTO booking)
        {
            try
            {
                // Tính tiền trước khi lưu
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
        ORDER BY d.GioBD"; // lấy booking sớm nhất hoặc mới nhất

            SqlParameter[] parameters = {
        new SqlParameter("@maSan", maSan),
        new SqlParameter("@ngayDat", ngayDat.Date)
    };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
        // ================= 4. KIỂM TRA TRÙNG GIỜ =================
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

        // ================= 5. LỌC SÂN =================
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
            // Lấy mã lớn nhất hiện có
            string query = "SELECT TOP 1 MaHD FROM hoadon ORDER BY MaHD DESC";
            object result = db.ExecuteScalar(query);

            if (result == null || result == DBNull.Value)
            {
                return "HD0001"; // Nếu chưa có hóa đơn nào
            }

            // Tách phần số ra khỏi chuỗi "HDxxxx"
            string currentMa = result.ToString(); // Ví dụ: "HD0005"
            int currentNumber = int.Parse(currentMa.Substring(2));
            int nextNumber = currentNumber + 1;

            // Trả về định dạng HD + 4 chữ số (ví dụ: HD0006)
            return "HD" + nextNumber.ToString("D4");
        }

        // ================= 6. LOAD THÔNG TIN =================
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

        // ================= 7. THANH TOÁN (FIX CHUẨN) =================
        public bool ThanhToan(string maDatSan, string maKH, double tongTien)
        {
            // 1. Tự tạo mã HD đúng định dạng HD + 4 số (ví dụ: HD0005)
            string queryMax = "SELECT TOP 1 MaHD FROM hoadon ORDER BY MaHD DESC";
            object result = db.ExecuteScalar(queryMax);
            string maHDmoi = "HD0001"; // Mặc định nếu chưa có HD nào

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
                    // UPDATE trạng thái đặt sân
                    SqlCommand updateCmd = new SqlCommand(
                        "UPDATE datsan SET TrangThai = N'Đã thanh toán' WHERE MaDatSan = @ma", conn, tran);
                    updateCmd.Parameters.AddWithValue("@ma", maDatSan);
                    updateCmd.ExecuteNonQuery();

                    // INSERT hóa đơn với mã mới tự tạo
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