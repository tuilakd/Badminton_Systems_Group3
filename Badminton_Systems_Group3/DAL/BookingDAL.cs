using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Badminton_Systems_Group3.DAL
{
    public class BookingDAL
    {
        // Khởi tạo đối tượng kết nối Database
        private readonly DatabaseHelper db = new DatabaseHelper();

        // ================== 1. LƯU KHÁCH HÀNG ==================
        // Phải gọi hàm này trước khi InsertBooking để tránh lỗi Foreign Key MaKH
        public bool InsertKhachHang(string maKH, string hoTen, string sdt)
        {
            try
            {
                // Kiểm tra xem khách hàng đã tồn tại dựa trên số điện thoại chưa
                string checkQuery = "SELECT COUNT(*) FROM khachhang WHERE SDT = @sdt";
                SqlParameter[] checkParams = { new SqlParameter("@sdt", sdt) };
                int exists = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));

                // Nếu khách đã có trong hệ thống, không cần chèn thêm (tránh trùng khóa chính MaKH)
                if (exists > 0) return true;

                string query = "INSERT INTO khachhang (MaKH, HoTen, SDT) VALUES (@makh, @hoten, @sdt)";
                SqlParameter[] parameters = {
                    new SqlParameter("@makh", maKH),
                    new SqlParameter("@hoten", hoTen),
                    new SqlParameter("@sdt", sdt)
                };
                return db.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi DAL InsertKhachHang: " + ex.Message);
                return false;
            }
        }
        public DataRow GetThongTinKhachDatSan(string maSan, DateTime ngay, DateTime batDau, DateTime ketThuc)
        {
            // db ở đây là thực thể DatabaseHelper bạn đã khai báo trong DAL
            string query = @"SELECT k.HoTen, k.SDT, d.MaDatSan, d.ThanhTien 
                     FROM datsan d 
                     JOIN khachhang k ON d.MaKH = k.MaKH 
                     WHERE d.MaSan = @maSan 
                     AND d.NgayDat = @ngay 
                     AND CAST(d.GioBD AS TIME) = CAST(@start AS TIME) 
                     AND CAST(d.GioKT AS TIME) = CAST(@end AS TIME)";

            SqlParameter[] parameters = {
        new SqlParameter("@maSan", maSan),
        new SqlParameter("@ngay", ngay),
        new SqlParameter("@start", batDau),
        new SqlParameter("@end", ketThuc)
    };

            DataTable dt = db.ExecuteQuery(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // ================== 2. LƯU ĐƠN ĐẶT SÂN ==================
        public bool InsertBooking(BookingDTO booking)
        {
            // Câu lệnh này đã bao gồm GiaThue và ThanhTien để giải quyết lỗi Invalid Column Name
            string query = @"INSERT INTO datsan (MaDatSan, NgayDat, GioBD, GioKT, TrangThai, MaKH, MaSan, GiaThue, ThanhTien) 
                             VALUES (@mads, @ngay, @giobd, @giokt, @tt, @makh, @masan, @gia, @thanhtien)";

            SqlParameter[] parameters = {
                new SqlParameter("@mads", booking.MaDatSan),
                new SqlParameter("@ngay", booking.NgayDat.Date),
                // Sử dụng SqlDbType.Time để tương thích chính xác với kiểu dữ liệu Time trong SQL
                new SqlParameter("@giobd", SqlDbType.Time) { Value = booking.GioBatDau.TimeOfDay },
                new SqlParameter("@giokt", SqlDbType.Time) { Value = booking.GioKetThuc.TimeOfDay },
                new SqlParameter("@tt", booking.TrangThai),
                new SqlParameter("@makh", booking.MaKH),
                new SqlParameter("@masan", booking.MaSan),
                new SqlParameter("@gia", booking.GiaThue),
                new SqlParameter("@thanhtien", booking.ThanhTien)
            };

            return db.ExecuteNonQuery(query, parameters);
        }

        // ================== 3. CẬP NHẬT TRẠNG THÁI SÂN ==================
        public bool UpdateSanStatus(string maSan, string status)
        {
            string query = "UPDATE san SET TrangThai = @status WHERE MaSan = @masan";
            SqlParameter[] parameters = {
                new SqlParameter("@status", status),
                new SqlParameter("@masan", maSan)
            };
            return db.ExecuteNonQuery(query, parameters);
        }

        // ================== 4. KIỂM TRA TRÙNG GIỜ ==================
        public bool KiemTraTrungGio(BookingDTO booking)
        {
            string query = @"SELECT COUNT(*) FROM datsan 
                             WHERE MaSan = @masan 
                             AND NgayDat = @ngaydat 
                             AND (@giobd < GioKT AND @giokt > GioBD)";

            SqlParameter[] parameters = {
                new SqlParameter("@masan", booking.MaSan),
                new SqlParameter("@ngaydat", booking.NgayDat.Date),
                new SqlParameter("@giobd", SqlDbType.Time) { Value = booking.GioBatDau.TimeOfDay },
                new SqlParameter("@giokt", SqlDbType.Time) { Value = booking.GioKetThuc.TimeOfDay }
            };

            int count = Convert.ToInt32(db.ExecuteScalar(query, parameters));
            return count > 0;
        }

        // ================== 5. LỌC DANH SÁCH SÂN ĐÃ BỊ ĐẶT ==================
        public DataTable GetSanDaDat(DateTime ngay, DateTime gioBD, DateTime gioKT)
        {
            string query = @"SELECT DISTINCT MaSan FROM datsan 
                             WHERE NgayDat = @ngaydat 
                             AND (@giobd < GioKT AND @giokt > GioBD)";

            SqlParameter[] parameters = {
                new SqlParameter("@ngaydat", ngay.Date),
                new SqlParameter("@giobd", SqlDbType.Time) { Value = gioBD.TimeOfDay },
                new SqlParameter("@giokt", SqlDbType.Time) { Value = gioKT.TimeOfDay }
            };

            return db.ExecuteQuery(query, parameters);
        }
    }
}