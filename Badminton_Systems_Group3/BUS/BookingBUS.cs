using System;
using System.Data;
using System.Linq;
using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;

namespace Badminton_Systems_Group3.BUS
{
    public class BookingBUS
    {
        private readonly BookingDAL dal = new BookingDAL();

        private string TaoMaKH(string sdt)
            => "KH" + (sdt?.Trim().Replace(" ", "") ?? "");

        private string TaoMaDatSan()
            => "DS" + DateTime.Now.ToString("ddHHmmss");

        private decimal TinhTien(TimeSpan start, TimeSpan end, decimal gia)
        {
            double h = (end - start).TotalHours;
            return h > 0 ? (decimal)h * gia : 0;
        }

        public decimal TinhTienPublic(TimeSpan start, TimeSpan end, decimal gia)
        {
            return TinhTien(start, end, gia);
        }

        private (bool, string) Validate(BookingDTO b)
        {
            if (b == null) return (false, "Dữ liệu null!");
            if (string.IsNullOrWhiteSpace(b.MaSan)) return (false, "Chọn sân!");
            if (string.IsNullOrWhiteSpace(b.TenKhachHang)) return (false, "Nhập tên!");
            if (string.IsNullOrWhiteSpace(b.SDT)) return (false, "Nhập SĐT!");
            if (!b.SDT.All(char.IsDigit) || b.SDT.Length < 9) return (false, "SĐT sai!");
            if (b.NgayDat.Date < DateTime.Today) return (false, "Ngày sai!");
            if (b.GioKetThuc <= b.GioBatDau) return (false, "Giờ sai!");
            if (b.GiaThue <= 0) return (false, "Giá sai!");
            return (true, "");
        }

        private (bool, string) CheckTT(string ma, params string[] invalid)
        {
            var tt = dal.GetTrangThaiBooking(ma)?.Trim();

            if (string.IsNullOrEmpty(tt))
                return (false, "Không tìm thấy!");

            if (invalid.Any(x => tt.Equals(x, StringComparison.OrdinalIgnoreCase)))
                return (false, $"Trạng thái '{tt}' không hợp lệ!");

            return (true, "");
        }

        public (bool, string) ThucHienDatSan(BookingDTO b)
        {
            var v = Validate(b);
            if (!v.Item1) return v;

            try
            {
                if (dal.KiemTraTrungGio(b))
                    return (false, "Trùng giờ!");

                b.MaKH = TaoMaKH(b.SDT);

                if (!dal.KhachHangTonTai(b.MaKH))
                {
                    bool ok = dal.InsertKhachHang(b.MaKH, b.TenKhachHang, b.SDT);
                    if (!ok) return (false, "Lỗi khách hàng!");
                }

                b.MaDatSan = TaoMaDatSan();
                b.TrangThai = "Đã đặt";
                b.ThanhTien = TinhTien(b.GioBatDau, b.GioKetThuc, b.GiaThue);

                bool result = dal.InsertBooking(b);

                return result
                    ? (true, "Đặt sân thành công!")
                    : (false, "Lưu thất bại!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        public (bool, string) ThanhToan(string ma, string sdt)
        {
            if (string.IsNullOrEmpty(ma)) return (false, "Chưa chọn!");
            if (string.IsNullOrWhiteSpace(sdt)) return (false, "Thiếu SĐT!");

            var c = CheckTT(ma, "Đã thanh toán", "Đã hủy");
            if (!c.Item1) return c;

            try
            {
                string makh = TaoMaKH(sdt);
                decimal tien = dal.GetTongTien(ma);

                if (tien <= 0)
                    return (false, "Tiền không hợp lệ!");

                bool result = dal.ThanhToan(ma, makh, (double)tien);

                return result
                    ? (true, "Thanh toán $ lưu hóa đơn thành công!")
                    : (false, "Thanh toán thất bại!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        public (bool, string) HuyLich(string ma)
        {
            if (string.IsNullOrEmpty(ma))
                return (false, "Chưa chọn!");

            var c = CheckTT(ma, "Đã thanh toán", "Đã hủy");
            if (!c.Item1) return c;

            bool result = dal.HuyLich(ma);

            return result
                ? (true, "Hủy thành công!")
                : (false, "Hủy thất bại!");
        }

        public (bool, string) UpdateBooking(string ma, DateTime ngay, TimeSpan bd, TimeSpan kt, string maSan, decimal gia)
        {
            if (kt <= bd)
                return (false, "Giờ không hợp lệ!");

            var c = CheckTT(ma, "Đã thanh toán", "Đã hủy");
            if (!c.Item1) return c;

            var temp = new BookingDTO
            {
                MaSan = maSan,
                NgayDat = ngay,
                GioBatDau = bd,
                GioKetThuc = kt
            };

            if (dal.KiemTraTrungGioUpdate(temp, ma))
                return (false, "Trùng lịch!");

            decimal tien = TinhTien(bd, kt, gia);

            bool result = dal.UpdateBooking(ma, ngay, bd, kt, (double)tien);

            return result
                ? (true, "Cập nhật thành công!")
                : (false, "Cập nhật thất bại!");
        }

        public DataTable LayLichDatSanFull()
            => dal.GetAllBookingSchedule();

        public DataTable GetSanDaDat(DateTime n, TimeSpan bd, TimeSpan kt)
            => dal.GetSanDaDat(n, bd, kt);

        public bool KiemTraSanDaDat(string s, DateTime n, TimeSpan bd, TimeSpan kt)
            => dal.GetThongTinKhachDatSanChuaThanhToan(s, n, bd, kt) != null;

        public DataTable SearchBooking(string tt, string kw, DateTime? n)
            => dal.SearchBooking(tt, kw, n);

        internal object UpdateBooking(string? maDatSan, DateTime ngay, TimeSpan bd, TimeSpan kt, string? maSan)
        {
            throw new NotImplementedException();
        }
    }
}