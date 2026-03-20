using System;
namespace Badminton_Systems_Group3.DTO
{

    public class ProductDTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuongTon { get; set; }
        public string HinhAnh { get; set; }
    }
    public class SalesDTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;

    }

}