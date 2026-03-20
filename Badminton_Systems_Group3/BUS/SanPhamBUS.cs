using System;
using System.Collections.Generic;
using System.Text;
using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;

namespace Badminton_Systems_Group3.BUS
{
    internal class SanPhamBUS
    {
        SanPhamDAL dal = new SanPhamDAL();

        public string ThucHienNhapKho(ProductDTO dto)
        {
            // Luồng ngoại lệ 6: Kiểm tra bỏ trống (đã xử lý 1 phần ở GUI nhưng BUS vẫn nên check lại)
            if (string.IsNullOrEmpty(dto.MaSP)) return "Mã sản phẩm không được để trống!";

            if (dal.CheckExists(dto.MaSP))
            {
                // Nếu tồn tại -> Cộng dồn (Luồng 3)
                return dal.Update(dto) ? "Cập nhật số lượng thành công!" : "Lỗi cập nhật.";
            }
            else
            {
                // Nếu chưa có -> Thêm mới
                return dal.Insert(dto) ? "Thêm mới sản phẩm thành công!" : "Lỗi thêm mới.";
            }
        }
    
    }
}
