using System;

namespace Badminton_Systems_Group3.DTO
{
    public class BookingDTO
    {
        // ====== KHÓA CHÍNH ======
        public string MaDatSan { get; set; } = string.Empty;

        // ====== KHÓA NGOẠI ======
        public string MaSan { get; set; } = string.Empty;
        public string MaKH { get; set; } = string.Empty;

        // ====== THÔNG TIN KHÁCH ======
        public string TenKhachHang { get; set; } = string.Empty;
        public string SDT { get; set; } = string.Empty;

        // ====== THỜI GIAN ======
        public DateTime NgayDat { get; set; } = DateTime.Now;

        // 🔥 FIX QUAN TRỌNG: dùng TimeSpan cho đúng SQL TIME
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }

        // ====== TRẠNG THÁI ======
        public string TrangThai { get; set; } = "Đã đặt";

        // ====== GIÁ ======
        public decimal GiaThue { get; set; } = 120000;

        // ====== THÀNH TIỀN ======
        public decimal ThanhTien { get; set; }

        // ====== HÀM TÍNH TIỀN ======
        public void TinhThanhTien()
        {
            double soGio = (GioKetThuc - GioBatDau).TotalHours;

            if (soGio <= 0)
            {
                ThanhTien = 0;
                return;
            }

            ThanhTien = (decimal)soGio * GiaThue;
        }
    }
}