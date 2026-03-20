using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using System;
using System.Data;

namespace Badminton_Systems_Group3.BUS
{
    internal class StorageBUS
    {
        StorageDAL dal = new StorageDAL();

        public DataTable GetAll()
        {
            return dal.GetAll();
        }

        public string ThucHienNhapKho(StorageDTO dto)
        {
            if (string.IsNullOrEmpty(dto.MaSP)) return "Mã sản phẩm không được để trống!";
            if (dto.SoLuongTon <= 0) return "Số lượng nhập phải lớn hơn 0!";

            if (dal.CheckMaSPExistsInDanhMuc(dto.MaSP))
            {
                return dal.InsertNhapKho(dto) ? "Nhập kho và cập nhật số lượng thành công!" : "Lỗi hệ thống khi nhập kho.";
            }
            else
            {
                return "Lỗi: Mã sản phẩm này chưa tồn tại trong danh mục. Vui lòng thêm sản phẩm mới trước!";
            }
        }
    }
}