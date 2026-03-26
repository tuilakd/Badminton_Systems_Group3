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

    public string TrangThai { get; set; } = string.Empty;

    public decimal GiaThue { get; set; }
    public decimal ThanhTien { get; set; }
}