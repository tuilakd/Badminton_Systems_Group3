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

        public (bool success, string message) ThucHienDatSan(BookingDTO booking)
        {
            if (string.IsNullOrWhiteSpace(booking.MaSan))
                return (false, "Vui lòng chọn sân!");

            if (string.IsNullOrWhiteSpace(booking.TenKhachHang))
                return (false, "Vui lòng nhập tên khách!");

            if (string.IsNullOrWhiteSpace(booking.SDT))
                return (false, "Vui lòng nhập số điện thoại!");

            if (!booking.SDT.All(char.IsDigit) || booking.SDT.Length < 9)
                return (false, "SĐT không hợp lệ!");

            if (booking.NgayDat.Date < DateTime.Today)
                return (false, "Ngày đặt không thể ở quá khứ!");

            if (booking.GioKetThuc <= booking.GioBatDau)
                return (false, "Giờ kết thúc phải lớn hơn giờ bắt đầu!");

            try
            {
                if (dal.KiemTraTrungGio(booking))
                    return (false, "Sân đã có người đặt trong khung giờ này!");

                booking.MaKH = "KH" + booking.SDT.Trim().Replace(" ", "");

                if (!dal.KhachHangTonTai(booking.MaKH))
                {
                    if (!dal.InsertKhachHang(booking.MaKH, booking.TenKhachHang, booking.SDT))
                        return (false, "Lỗi lưu khách hàng!");
                }

                booking.MaDatSan = "DS" + DateTime.Now.ToString("ddHHmmss");

                booking.TinhThanhTien();

                if (!dal.InsertBooking(booking))
                    return (false, "Lưu đặt sân thất bại!");

                return (true, "Đặt sân thành công!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống: " + ex.Message);
            }
        }

        public DataTable GetSanDaDat(DateTime ngay, TimeSpan gioBD, TimeSpan gioKT)
        {
            return dal.GetSanDaDat(ngay, gioBD, gioKT);
        }

        public (bool success, string message) ThanhToan(string maDatSan, string sdt, decimal tongTien)
        {
            if (string.IsNullOrEmpty(maDatSan))
                return (false, "Chưa chọn dữ liệu thanh toán!");

            if (string.IsNullOrWhiteSpace(sdt))
                return (false, "Thiếu số điện thoại!");

            if (tongTien <= 0)
                return (false, "Số tiền không hợp lệ!");

            try
            {
                string maKH = "KH" + sdt.Trim();
                string maHD = "HD" + DateTime.Now.ToString("ddHHmmss");

                bool ok = dal.ThanhToan(maDatSan, maKH, (double)tongTien);
                if (!ok)
                    return (false, "Thanh toán thất bại!");

                return (true, "Thanh toán thành công!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống: " + ex.Message);
            }
        }
        public bool KiemTraSanDaDat(string maSan, DateTime ngay)
        {
            var dt = dal.GetThongTinKhachDatSanChuaThanhToan(maSan, ngay);
            return dt != null; 
        }
        public decimal TinhTien(TimeSpan start, TimeSpan end, decimal giaMoiGio = 120000)
        {
            var duration = end - start;
            return duration.TotalHours > 0 ? (decimal)duration.TotalHours * giaMoiGio : 0;
        }
    }
}