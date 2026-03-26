using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Badminton_Systems_Group3.DAL
{
    public class BookingDAL
    {
        private readonly DatabaseHelper db = new DatabaseHelper();

        private SqlParameter P(string name, object value)
            => new SqlParameter(name, value ?? DBNull.Value);

        private SqlParameter PT(string name, SqlDbType type, object value)
            => new SqlParameter(name, type) { Value = value ?? DBNull.Value };

        public bool InsertKhachHang(string maKH, string hoTen, string sdt)
        {
            try
            {
                var exists = db.ExecuteScalar(
                    "SELECT COUNT(*) FROM khachhang WHERE SDT = @sdt",
                    new[] { P("@sdt", sdt) });

                if (Convert.ToInt32(exists) > 0) return true;

                return db.ExecuteNonQuery(
                    "INSERT INTO khachhang (MaKH, HoTen, SDT) VALUES (@makh, @hoten, @sdt)",
                    new[] {
                        P("@makh", maKH),
                        P("@hoten", hoTen),
                        P("@sdt", sdt)
                    });
            }
            catch { return false; }
        }

        public bool KhachHangTonTai(string maKH)
        {
            var r = db.ExecuteScalar(
                "SELECT COUNT(*) FROM khachhang WHERE MaKH = @ma",
                new[] { P("@ma", maKH) });

            return Convert.ToInt32(r) > 0;
        }

        public bool InsertBooking(BookingDTO b)
        {
            try
            {
                var gia = db.ExecuteScalar(
                    "SELECT GiaThue FROM san WHERE MaSan = @ma",
                    new[] { P("@ma", b.MaSan) });

                b.GiaThue = gia != DBNull.Value ? Convert.ToDecimal(gia) : 0;

                return db.ExecuteNonQuery(
                    @"INSERT INTO datsan 
            (MaDatSan, NgayDat, GioBD, GioKT, TrangThai, MaKH, MaSan, GiaThue, ThanhTien)
            VALUES (@mads, @ngay, @bd, @kt, @tt, @makh, @masan, @gia, @tien)",
                    new[] {
                P("@mads", b.MaDatSan),
                P("@ngay", b.NgayDat.Date),
                PT("@bd", SqlDbType.Time, b.GioBatDau),
                PT("@kt", SqlDbType.Time, b.GioKetThuc),
                P("@tt", b.TrangThai),
                P("@makh", b.MaKH),
                P("@masan", b.MaSan),
                P("@gia", b.GiaThue),
                P("@tien", b.ThanhTien)
                    });
            }
            catch { return false; }
        }

        public DataRow GetThongTinKhachDatSanChuaThanhToan(string maSan, DateTime ngay, TimeSpan bd, TimeSpan kt)
        {
            var dt = db.ExecuteQuery(
                @"SELECT ds.MaDatSan, kh.HoTen, kh.SDT, ds.GioBD, ds.GioKT, ds.ThanhTien
                  FROM datsan ds
                  JOIN khachhang kh ON ds.MaKH = kh.MaKH
                  WHERE ds.MaSan = @san AND ds.NgayDat = @ngay
                  AND ds.TrangThai = N'Đã đặt'
                  AND ds.GioBD = @bd AND ds.GioKT = @kt",
                new[] {
                    P("@san", maSan),
                    P("@ngay", ngay),
                    PT("@bd", SqlDbType.Time, bd),
                    PT("@kt", SqlDbType.Time, kt)
                });

            return dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public bool KiemTraTrungGio(BookingDTO b)
        {
            var r = db.ExecuteScalar(
                @"SELECT COUNT(*) FROM datsan 
                  WHERE MaSan=@san AND CAST(NgayDat AS DATE)=@ngay
                  AND (@bd < GioKT AND @kt > GioBD)
                  AND TrangThai <> N'Đã hủy'",
                new[] {
                    P("@san", b.MaSan),
                    P("@ngay", b.NgayDat.Date),
                    PT("@bd", SqlDbType.Time, b.GioBatDau),
                    PT("@kt", SqlDbType.Time, b.GioKetThuc)
                });

            return Convert.ToInt32(r) > 0;
        }

        public DataTable GetSanDaDat(DateTime ngay, TimeSpan bd, TimeSpan kt)
        {
            return db.ExecuteQuery(
                @"SELECT DISTINCT MaSan FROM datsan
                  WHERE CAST(NgayDat AS DATE)=@ngay
                  AND (@bd < GioKT AND @kt > GioBD)
                  AND TrangThai=N'Đã đặt'",
                new[] {
                    P("@ngay", ngay.Date),
                    PT("@bd", SqlDbType.Time, bd),
                    PT("@kt", SqlDbType.Time, kt)
                });
        }

        public string GetNewMaHD()
        {
            var r = db.ExecuteScalar("SELECT TOP 1 MaHD FROM hoadon ORDER BY MaHD DESC");

            if (r == null || r == DBNull.Value) return "HD0001";

            string current = r.ToString();
            if (current.StartsWith("HD") && int.TryParse(current.Substring(2), out int num))
                return "HD" + (num + 1).ToString("D4");

            return "HD0001";
        }

        public DataTable SearchBooking(string trangThai, string keyword, DateTime? ngay)
        {
            string query = @"SELECT ds.MaDatSan, s.TenSan, ds.TrangThai, kh.HoTen, kh.SDT,
                             ds.NgayDat, ds.GioBD, ds.GioKT
                             FROM datsan ds
                             JOIN khachhang kh ON ds.MaKH = kh.MaKH
                             JOIN san s ON ds.MaSan = s.MaSan WHERE 1=1";

            var p = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                query += " AND ds.TrangThai=@tt";
                p.Add(P("@tt", trangThai));
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query += " AND (kh.HoTen LIKE @kw OR kh.SDT LIKE @kw OR s.TenSan LIKE @kw)";
                p.Add(P("@kw", "%" + keyword + "%"));
            }

            if (ngay.HasValue)
            {
                query += " AND CAST(ds.NgayDat AS DATE)=@ngay";
                p.Add(P("@ngay", ngay.Value.Date));
            }

            query += " ORDER BY ds.NgayDat DESC, ds.GioBD ASC";

            return db.ExecuteQuery(query, p.ToArray());
        }

        public bool HuyLich(string ma)
        {
            return db.ExecuteNonQuery(
                "UPDATE datsan SET TrangThai=N'Đã hủy' WHERE MaDatSan=@ma",
                new[] { P("@ma", ma) });
        }

        public bool UpdateBooking(string ma, DateTime ngay, TimeSpan bd, TimeSpan kt, double tien)
        {
            return db.ExecuteNonQuery(
                @"UPDATE datsan 
          SET NgayDat=@ngay, GioBD=@bd, GioKT=@kt, ThanhTien=@tien 
          WHERE MaDatSan=@ma",
                new[] {
            P("@ngay", ngay.Date),
            PT("@bd", SqlDbType.Time, bd),
            PT("@kt", SqlDbType.Time, kt),
            P("@tien", tien),
            P("@ma", ma)
                });
        }
        public decimal GetTongTien(string ma)
        {
            var r = db.ExecuteScalar(
                "SELECT ThanhTien FROM datsan WHERE MaDatSan=@ma",
                new[] { P("@ma", ma) });

            return r != null && r != DBNull.Value ? Convert.ToDecimal(r) : 0;
        }

        public DataRow GetThongTinKhachDatSan(string maSan, DateTime ngay, DateTime bd, DateTime kt)
        {
            var dt = db.ExecuteQuery(
                @"SELECT k.HoTen, k.SDT, d.MaDatSan, d.ThanhTien
                  FROM datsan d
                  JOIN khachhang k ON d.MaKH = k.MaKH
                  WHERE d.MaSan=@san AND CAST(d.NgayDat AS DATE)=@ngay
                  AND d.GioBD=@bd AND d.GioKT=@kt",
                new[] {
                    P("@san", maSan),
                    P("@ngay", ngay.Date),
                    PT("@bd", SqlDbType.Time, bd.TimeOfDay),
                    PT("@kt", SqlDbType.Time, kt.TimeOfDay)
                });

            return dt != null && dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public DataTable GetAllBookingSchedule()
        {
            return db.ExecuteQuery(
                @"SELECT d.MaDatSan, d.MaSan, s.TenSan, d.TrangThai, kh.HoTen, kh.SDT,
                  d.NgayDat, d.GioBD, d.GioKT
                  FROM datsan d
                  JOIN san s ON d.MaSan = s.MaSan
                  JOIN khachhang kh ON d.MaKH = kh.MaKH
                  ORDER BY d.NgayDat DESC, d.GioBD ASC");
        }

        public string GetTrangThaiBooking(string ma)
        {
            var r = db.ExecuteScalar(
                "SELECT TrangThai FROM datsan WHERE MaDatSan=@ma",
                new[] { P("@ma", ma) });

            return r?.ToString().Trim() ?? "";
        }

        public bool KiemTraTrungGioUpdate(BookingDTO b, string ma)
        {
            var r = db.ExecuteScalar(
                @"SELECT COUNT(*) FROM datsan 
                  WHERE MaSan=@san AND CAST(NgayDat AS DATE)=@ngay
                  AND (@bd < GioKT AND @kt > GioBD)
                  AND TrangThai <> N'Đã hủy'
                  AND MaDatSan <> @ma",
                new[] {
                    P("@san", b.MaSan),
                    P("@ngay", b.NgayDat.Date),
                    PT("@bd", SqlDbType.Time, b.GioBatDau),
                    PT("@kt", SqlDbType.Time, b.GioKetThuc),
                    P("@ma", ma)
                });

            return Convert.ToInt32(r) > 0;
        }

        public bool ThanhToan(string maDatSan, string maKH, double tongTien)
        {
            string maHD = GetNewMaHD();

            using (SqlConnection conn = db.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    new SqlCommand("UPDATE datsan SET TrangThai=N'Đã thanh toán' WHERE MaDatSan=@ma", conn, tran)
                    { Parameters = { new SqlParameter("@ma", maDatSan) } }.ExecuteNonQuery();

                    new SqlCommand(
                        "INSERT INTO hoadon (MaHD, MaKH, NgayLapHD, TongTien) VALUES (@hd,@kh,@ngay,@tien)",
                        conn, tran)
                    {
                        Parameters = {
                            new SqlParameter("@hd", maHD),
                            new SqlParameter("@kh", maKH),
                            new SqlParameter("@ngay", DateTime.Now),
                            new SqlParameter("@tien", tongTien)
                        }
                    }.ExecuteNonQuery();

                    new SqlCommand(
                        @"INSERT INTO chitiethoadon_san (MaHD, MaDatSan, GiaThue, ThanhTien)
                          SELECT @hd, MaDatSan,
                          (SELECT GiaThue FROM san WHERE san.MaSan=datsan.MaSan),
                          ThanhTien FROM datsan WHERE MaDatSan=@ma",
                        conn, tran)
                    {
                        Parameters = {
                            new SqlParameter("@hd", maHD),
                            new SqlParameter("@ma", maDatSan)
                        }
                    }.ExecuteNonQuery();

                    new SqlCommand(
                        @"UPDATE san SET TrangThai=N'Trống'
                          WHERE MaSan=(SELECT MaSan FROM datsan WHERE MaDatSan=@ma)",
                        conn, tran)
                    { Parameters = { new SqlParameter("@ma", maDatSan) } }.ExecuteNonQuery();

                    tran.Commit();
                    return true;
                }
                catch
                {
                    tran.Rollback();
                    return false;
                }
            }
        }
    }
}