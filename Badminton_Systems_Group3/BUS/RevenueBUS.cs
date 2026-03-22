using System;
using System.Data;
using Badminton_Systems_Group3.DAL;

namespace Badminton_Systems_Group3.BUS
{
    public class RevenueBUS
    {
        private RevenueDAL dal = new RevenueDAL();

        public decimal GetDoanhThu(string type)
        {
            return dal.GetDoanhThu(type);
        }

        public DataTable GetDanhSachGiaoDich(string tuNgay, string denNgay, string loaiHD)
        {
            return dal.GetDanhSachGiaoDich(tuNgay, denNgay, loaiHD);
        }

        public bool LuuBaoCaoDoanhThu(decimal tongTien)
        {
            return dal.LuuBaoCaoDoanhThu(tongTien);
        }
    }
}