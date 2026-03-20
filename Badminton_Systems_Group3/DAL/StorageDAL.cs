using Badminton_Systems_Group3.Database;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Badminton_Systems_Group3.DAL
{
    internal class StorageDAL
    {
        DatabaseHelper helper = new DatabaseHelper();
        public DataTable GetAll()
        {
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

        public bool CheckMaSPExistsInDanhMuc(string maSP)
        {
            string query = $"SELECT MaSP FROM sanpham WHERE MaSP = '{maSP}'";
            DataTable dt = helper.GetData(query);
            return dt.Rows.Count > 0;
        }

        public bool InsertNhapKho(StorageDTO dto)
        {
            string donGia = dto.DonGia.ToString().Replace(",", ".");

            string query = $@"INSERT INTO nhapkho (MaSP, DonGia, SoLuongNhap, NgayNhap) 
                             VALUES ('{dto.MaSP}', {donGia}, {dto.SoLuongTon}, '{dto.NgayNhap:yyyy-MM-dd}')";

            bool result = helper.ExecuteNonQuery(query);

            if (result)
            {
                CapNhatTonKhoTong(dto.MaSP, dto.SoLuongTon);
            }
            return result;
        }

        public void CapNhatTonKhoTong(string maSP, int soLuongThem)
        {
            string query = $"UPDATE sanpham SET SoLuongTon = SoLuongTon + {soLuongThem} WHERE MaSP = '{maSP}'";
            helper.ExecuteNonQuery(query);
        }
    }
}
