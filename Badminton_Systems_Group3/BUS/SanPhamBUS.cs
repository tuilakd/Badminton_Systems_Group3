using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using System;
using System.Data;

namespace Badminton_Systems_Group3.BUS
{
    internal class SanPhamBUS
    {
        SanPhamDAL dal = new SanPhamDAL();

        public DataTable GetAll()
        {
            return dal.GetAll();
        }

        public string ThucHienNhapKho(ProductDTO dto)
        {
            // 1. Kiểm tra bỏ trống
            if (string.IsNullOrEmpty(dto.MaSP)) return "Mã sản phẩm không được để trống!";
            if (dto.SoLuongTon <= 0) return "Số lượng nhập phải lớn hơn 0!";

            // 2. Kiểm tra xem mã SP đã có trong danh mục (bảng sanpham) chưa
            // Thay vì dùng CheckExists cũ, ta dùng hàm mới ở DAL
            if (dal.CheckMaSPExistsInDanhMuc(dto.MaSP))
            {
                // Nếu đã có trong danh mục -> Tiến hành ghi vào nhật ký nhập kho
                // Hàm InsertNhapKho này bên trong DAL đã tự gọi hàm cộng dồn TonKho rồi
                return dal.InsertNhapKho(dto) ? "Nhập kho và cập nhật số lượng thành công!" : "Lỗi hệ thống khi nhập kho.";
            }
            else
            {
                // 3. Nếu chưa có trong danh mục -> Bắt người dùng đi khai báo SP trước
                // Để tránh lỗi Foreign Key (Khóa ngoại)
                return "Lỗi: Mã sản phẩm này chưa tồn tại trong danh mục. Vui lòng thêm sản phẩm mới trước!";
            }
        }
    }
}