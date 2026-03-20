using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System;
using System.Data;

namespace Badminton_Systems_Group3.DAL
{
    internal class SanPhamDAL
    {
        DatabaseHelper helper = new DatabaseHelper();

        // 1. Lấy dữ liệu: JOIN để có TenSP và ép tên cột SoLuongTon cho GUI dễ đọc
        public DataTable GetAll()
        {
            // GROUP BY MaSP và TenSP để gộp dòng
            // SUM để cộng dồn tất cả các lần nhập
            // MAX để lấy ngày nhập gần đây nhất
            string query = @"SELECT 
                        nk.MaSP, 
                        sp.TenSP, 
                        MAX(nk.DonGia) AS DonGia, 
                        SUM(nk.SoLuongNhap) AS TonKho, 
                        MAX(nk.NgayNhap) AS NgayNhap
                     FROM nhapkho nk
                     INNER JOIN sanpham sp ON nk.MaSP = sp.MaSP
                     GROUP BY nk.MaSP, sp.TenSP";

            return helper.GetData(query);
        }

        // 2. Kiểm tra danh mục sản phẩm (Bảng Cha)
        public bool CheckMaSPExistsInDanhMuc(string maSP)
        {
            string query = $"SELECT MaSP FROM sanpham WHERE MaSP = '{maSP}'";
            DataTable dt = helper.GetData(query);
            return dt.Rows.Count > 0;
        }

        // 3. Thêm bản ghi vào lịch sử NHAPKHO (Bảng Con)
        public bool InsertNhapKho(ProductDTO dto)
        {
            // Ép kiểu số để SQL không bị loạn dấu phẩy/dấu chấm
            string donGia = dto.DonGia.ToString().Replace(",", ".");

            string query = $@"INSERT INTO nhapkho (MaSP, DonGia, SoLuongNhap, NgayNhap) 
                             VALUES ('{dto.MaSP}', {donGia}, {dto.SoLuongTon}, '{dto.NgayNhap:yyyy-MM-dd}')";

            bool result = helper.ExecuteNonQuery(query);

            // Nếu lưu lịch sử xong thì cập nhật tổng kho ở bảng sanpham
            if (result)
            {
                CapNhatTonKhoTong(dto.MaSP, dto.SoLuongTon);
            }
            return result;
        }

        // 4. Cập nhật số lượng tồn vào bảng gốc
        public void CapNhatTonKhoTong(string maSP, int soLuongThem)
        {
            string query = $"UPDATE sanpham SET SoLuongTon = SoLuongTon + {soLuongThem} WHERE MaSP = '{maSP}'";
            helper.ExecuteNonQuery(query);
        }
    }
}