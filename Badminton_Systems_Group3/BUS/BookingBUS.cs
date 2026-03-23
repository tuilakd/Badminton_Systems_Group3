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

                // Tạo mã khách hàng dựa trên SĐT
                booking.MaKH = "KH" + booking.SDT.Trim().Replace(" ", "");

                if (!dal.KhachHangTonTai(booking.MaKH))
                {
                    if (!dal.InsertKhachHang(booking.MaKH, booking.TenKhachHang, booking.SDT))
                        return (false, "Lỗi lưu thông tin khách hàng mới!");
                }

                // Tạo mã đặt sân duy nhất
                booking.MaDatSan = "DS" + DateTime.Now.ToString("ddHHmmss");

                booking.TinhThanhTien();

                if (!dal.InsertBooking(booking))
                    return (false, "Lưu thông tin đặt sân thất bại!");

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
                return (false, "Thiếu số điện thoại khách hàng!");

            if (tongTien <= 0)
                return (false, "Số tiền thanh toán không hợp lệ!");

            try
            {
                string maKH = "KH" + sdt.Trim().Replace(" ", "");

                // Thực hiện lưu hóa đơn và cập nhật trạng thái đặt sân sang 'Đã thanh toán'
                bool ok = dal.ThanhToan(maDatSan, maKH, (double)tongTien);

                if (!ok)
                    return (false, "Quá trình thanh toán thất bại tại lớp dữ liệu!");

                return (true, "Thanh toán thành công!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống khi thanh toán: " + ex.Message);
            }
        }

        public (bool success, string message) HuyLich(string maDatSan)
        {
            if (string.IsNullOrEmpty(maDatSan))
                return (false, "Vui lòng chọn lịch cần hủy!");

            bool ok = dal.HuyLich(maDatSan);
            return ok ? (true, "Hủy lịch thành công!") : (false, "Không thể hủy lịch này!");
        }

        public (bool success, string message) UpdateBooking(string maDatSan, DateTime ngay, TimeSpan bd, TimeSpan kt, string maSan)
        {
            if (kt <= bd)
                return (false, "Giờ kết thúc phải lớn hơn giờ bắt đầu!");

            BookingDTO temp = new BookingDTO
            {
                MaSan = maSan,
                NgayDat = ngay,
                GioBatDau = bd,
                GioKetThuc = kt
            };

            if (dal.KiemTraTrungGioUpdate(temp, maDatSan))
                return (false, "Sân đã có lịch khác trong khung giờ mới chọn!");

            bool ok = dal.UpdateBooking(maDatSan, ngay, bd, kt);
            return ok ? (true, "Thay đổi lịch thành công!") : (false, "Thay đổi lịch thất bại!");
        }

        public DataTable LayLichDatSanFull()
        {
            return dal.GetAllBookingSchedule();
        }

        // ================= KIỂM TRA SÂN =================

        /// <summary>
        /// Kiểm tra xem một sân cụ thể đã được đặt trong khung giờ nhất định hay chưa
        /// </summary>
        public bool KiemTraSanDaDat(string maSan, DateTime ngay, TimeSpan gioBD, TimeSpan gioKT)
        {
            // Lấy thông tin từ DAL
            DataRow dr = dal.GetThongTinKhachDatSanChuaThanhToan(maSan, ngay, gioBD, gioKT);

            // Nếu dr khác null nghĩa là đã có người đặt (trả về true)
            return dr != null;
        }

        public decimal TinhTien(TimeSpan start, TimeSpan end, decimal giaMoiGio = 120000)
        {
            double duration = (end - start).TotalHours;
            return duration > 0 ? (decimal)duration * giaMoiGio : 0;
        }

        public DataTable SearchBooking(string trangThai, string keyword, DateTime? ngay)
        {
            return dal.SearchBooking(trangThai, keyword, ngay);
        }
    }
}