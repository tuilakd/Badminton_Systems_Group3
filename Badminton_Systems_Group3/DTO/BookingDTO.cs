using System;

namespace Badminton_Systems_Group3.DTO
{
    public class BookingDTO
    {
        // Mã đặt sân (Khóa chính bảng datsan)
        public string MaDatSan { get; set; } = string.Empty;

        // Mã sân (Khóa ngoại tham chiếu bảng san - Tag: SB0001)
        public string MaSan { get; set; } = string.Empty;

        // Mã khách hàng (Khóa ngoại tham chiếu bảng khachhang)
        public string MaKH { get; set; } = string.Empty;

        // Các thông tin bổ trợ để hiển thị hoặc lưu bảng khách hàng
        public string TenKhachHang { get; set; } = string.Empty;
        public string SDT { get; set; } = string.Empty;

        // Thông tin thời gian
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public DateTime GioBatDau { get; set; }
        public DateTime GioKetThuc { get; set; }

        // Trạng thái (Mặc định: Đã đặt)
        public string TrangThai { get; set; } = "Đã đặt";

        // Logic tính tiền: 120.000 VNĐ mỗi giờ
        public decimal GiaThue { get; set; } = 120000;

        // Thành tiền = (Giờ kết thúc - Giờ bắt đầu) * GiaThue
        public decimal ThanhTien { get; set; }
    }
}