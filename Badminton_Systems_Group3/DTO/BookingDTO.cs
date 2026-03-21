using System;

namespace Badminton_Systems_Group3.DTO
{
    public class BookingDTO
    {
        public string MaDatSan { get; set; } = string.Empty;

        public string MaSan { get; set; } = string.Empty;
        public string MaKH { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string SDT { get; set; } = string.Empty;

        public DateTime NgayDat { get; set; } = DateTime.Now;

        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }

        public string TrangThai { get; set; } = "Đã đặt";

        public decimal GiaThue { get; set; } = 120000;

        public decimal ThanhTien { get; set; }

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