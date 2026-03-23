using System;
using System.Collections.Generic;
using System.Text;

namespace Badminton_Systems_Group3.DTO
{
    public class StockEntryDTO
    {
        public string MaSP { get; set; }         
        public decimal DonGia { get; set; }     
        public int SoLuongNhap { get; set; }     
        public DateTime NgayNhap { get; set; }
    }
}
