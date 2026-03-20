using System;
using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;

namespace Badminton_Systems_Group3.BUS
{
    public class BookingBUS
    {
        private readonly BookingDAL dal = new BookingDAL();

        public string ThucHienDatSan(BookingDTO booking)
        {
            // ===== 1. Validate dữ liệu =====
            if (string.IsNullOrWhiteSpace(booking.MaSan))
                return "Vui lòng chọn sân!";

            if (string.IsNullOrWhiteSpace(booking.TenKhachHang))
                return "Vui lòng nhập tên khách!";

            if (string.IsNullOrWhiteSpace(booking.SDT))
                return "Vui lòng nhập số điện thoại!";

            if (booking.NgayDat.Date < DateTime.Today)
                return "Ngày đặt không hợp lệ!";

            // ===== 2. Validate giờ =====
            TimeSpan start = booking.GioBatDau.TimeOfDay;
            TimeSpan end = booking.GioKetThuc.TimeOfDay;

            if (end <= start)
                return "Giờ kết thúc phải lớn hơn giờ bắt đầu!";

            // ===== 3. Check trùng giờ =====
            if (dal.KiemTraTrungGio(booking))
                return "Sân đã có người đặt trong khung giờ này!";

            try
            {
                // ===== 4. Lưu DB =====
                bool insertOK = dal.InsertBooking(booking);

                if (!insertOK)
                    return "Lỗi khi lưu dữ liệu!";

                // ===== 5. Update trạng thái sân =====
                dal.UpdateSanStatus(booking.MaSan, "Đã đặt");

                return "Đặt sân thành công!";
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống: " + ex.Message;
            }
        }

        // ===== BONUS: Tính tiền =====
        public double TinhTien(BookingDTO booking, double giaMoiGio = 120000)
        {
            TimeSpan time = booking.GioKetThuc - booking.GioBatDau;
            return time.TotalHours * giaMoiGio;
        }
    }
}