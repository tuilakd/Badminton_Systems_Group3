using Badminton_Systems_Group3.DAL;
using System;
using System.Data;
using System.IO;
using System.Text;

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

        public Tuple<bool, string> XuatFileExcel(DataTable dt, string filePath)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return new Tuple<bool, string>(false, "Không có dữ liệu để xuất báo cáo!");
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(true)))
                {
                    sw.WriteLine("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                    sw.WriteLine("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" /></head>");
                    sw.WriteLine("<body>");

                    sw.WriteLine("<table border='1'>");

                    sw.WriteLine("<tr style='background-color: #4CAF50; color: white; font-weight: bold;'>");
                    sw.WriteLine("<th>Mã HD</th><th>Ngày lập</th><th>Loại</th><th>Chi tiết</th><th>Đơn giá</th><th>Số lượng</th><th>Thành tiền</th>");
                    sw.WriteLine("</tr>");

                    foreach (DataRow row in dt.Rows)
                    {
                        string maHD = row["MaHD"].ToString();
                        string ngayLap = Convert.ToDateTime(row["NgayLapHD"]).ToString("dd/MM/yyyy HH:mm");
                        string loai = row["LoaiHoaDon"].ToString();
                        string chiTiet = row["ChiTiet"].ToString();

                        string donGia = Convert.ToDecimal(row["DonGia"]).ToString("N0");
                        string soLuong = row["SoLuong"].ToString();
                        string thanhTien = Convert.ToDecimal(row["ThanhTien"]).ToString("N0");

                        sw.WriteLine("<tr>");
                        sw.WriteLine($"<td>{maHD}</td><td>{ngayLap}</td><td>{loai}</td><td>{chiTiet}</td><td>{donGia}</td><td>{soLuong}</td><td>{thanhTien}</td>");
                        sw.WriteLine("</tr>");
                    }

                    sw.WriteLine("</table>");
                    sw.WriteLine("</body>");
                    sw.WriteLine("</html>");
                }
                return new Tuple<bool, string>(true, "Đã xuất báo cáo thành công!");
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, "Lỗi khi xuất file: " + ex.Message);
            }
        }

        public Tuple<bool, string> KiemTraNgayTimKiem(DateTime? tuNgay, DateTime? denNgay)
        {
            if (tuNgay.HasValue && denNgay.HasValue)
            {
                if (tuNgay.Value > denNgay.Value)
                {
                    return new Tuple<bool, string>(false, "Lỗi: 'Từ ngày' không thể lớn hơn 'Đến ngày'. Vui lòng chọn lại!");
                }
            }
            return new Tuple<bool, string>(true, "");
        }
    }
}