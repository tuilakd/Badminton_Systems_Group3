using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using System.Collections.Generic;
using System.Data;
namespace Badminton_Systems_Group3.BUS
{
    public class WareHouseBUS
    {
        StorageDAL storageDAL = new StorageDAL();
        StockEntryDAL entryDAL = new StockEntryDAL();

        public DataTable LayKho(List<string> danhMucs) => storageDAL.LayDanhSach(danhMucs);

        public string XuLyGiaoDich(StockEntryDTO entry, bool laNhapMoi)
        {
            if (string.IsNullOrEmpty(entry.MaSP) || entry.SoLuongNhap <= 0)
                return "Vui lòng nhập đầy đủ Mã SP và Số lượng hợp lệ!";

            // laNhapMoi = true thì cộng (+), false thì trừ (-) để sửa lỗi nhập nhầm
            int delta = laNhapMoi ? entry.SoLuongNhap : -entry.SoLuongNhap;

            if (storageDAL.CapNhatTonKho(entry.MaSP, delta, entry.DonGia))
            {
                if (laNhapMoi) entryDAL.ThemPhieuNhap(entry);
                return laNhapMoi ? "Nhập kho thành công!" : "Đã trừ số lượng nhập nhầm!";
            }
            return "Thao tác thất bại. Vui lòng kiểm tra lại Mã SP!";
        }
    }
}