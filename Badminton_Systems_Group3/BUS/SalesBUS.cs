using System.Collections.Generic;
using System.Linq;
using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
namespace Badminton_Systems_Group3.BUS
{
    public class SalesBUS
    {
        private SalesDAL dal = new SalesDAL();
        public List<ProductDTO> GetAllProducts() => dal.GetProducts();
        public string CheckInventory(string maSP, int requestedQty)
        {
            var products = dal.GetProducts();
            var p = products.FirstOrDefault(x => x.MaSP == maSP);
            if (p == null || p.SoLuongTon < requestedQty)
            {
                return "Sản phẩm đã hết hoặc không đủ số lượng trong kho";
            }
            return null;
        }
        public bool SaveBillDetail(string maHD, SalesDTO item)
        {
            return dal.SaveChiTietHoaDon(maHD, item);
        }
        public bool CreateHoaDon(string maHD, decimal tongTien)
        {
            return dal.CreateHoaDon(maHD, tongTien);
        }
        public bool UpdateInventory(string maSP, int qty)
        {
            return dal.UpdateStock(maSP, qty);
        }
    }

}