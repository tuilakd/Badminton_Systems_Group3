using Badminton_Systems_Group3.BUS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Badminton_Systems_Group3.GUI
{
    /// <summary>
    /// Interaction logic for Revenue.xaml
    /// </summary>
    public partial class Revenue : Window
    {
        private RevenueBUS bus = new RevenueBUS();
        public Revenue()
        {
            InitializeComponent();
            LoadThongKe();
            LoadDanhSachGiaoDich();
        }

        private void btnThongTin_Click(object sender, RoutedEventArgs e)
        {
            Badminton_Systems_Group3.GUI.Info thongTinWindow = new Badminton_Systems_Group3.GUI.Info();
            thongTinWindow.ShowDialog();

            btnThongTin.IsChecked = false;
        }

        private void btnLichDat_Checked(object sender, RoutedEventArgs e)
        {
            BookingSchedule window = new BookingSchedule();
            window.Show();
            this.Close();
        }

        private void btnTrangChu_Checked(object sender, RoutedEventArgs e)
        {
            Home window = new Home();
            window.Show();
            this.Close();
        }

        private void LoadThongKe()
        {
            try
            {
                txtTongDoanhThu.Text = string.Format("{0:N0} VNĐ", bus.GetDoanhThu("Tong"));
                txtHomNay.Text = string.Format("{0:N0} VNĐ", bus.GetDoanhThu("HomNay"));
                txtThangNay.Text = string.Format("{0:N0} VNĐ", bus.GetDoanhThu("ThangNay"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thống kê: " + ex.Message);
            }
        }

        private void LoadDanhSachGiaoDich(string tuNgay = "", string denNgay = "", string loaiHoaDon = "Tất cả")
        {
            try
            {
                DataTable dt = bus.GetDanhSachGiaoDich(tuNgay, denNgay, loaiHoaDon);
                dgvDoanhThu.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message);
            }
        }

        private void btnBaoCao_Click(object sender, RoutedEventArgs e)
        {
            decimal tongTien = bus.GetDoanhThu("Tong");

            if (tongTien <= 0)
            {
                MessageBox.Show("Chưa có doanh thu để báo cáo!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (bus.LuuBaoCaoDoanhThu(tongTien))
            {
                XuatFileExcel();
            }
            else
            {
                MessageBox.Show("Lỗi khi lưu báo cáo vào CSDL!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string tuNgay = dpTuNgay.SelectedDate.HasValue ? dpTuNgay.SelectedDate.Value.ToString("yyyy-MM-dd") : "";
            string denNgay = dpDenNgay.SelectedDate.HasValue ? dpDenNgay.SelectedDate.Value.ToString("yyyy-MM-dd") : "";

            string loaiHD = cboLoaiHoaDon.Text.Trim();

            if (string.IsNullOrEmpty(loaiHD))
            {
                loaiHD = "Tất cả";
            }

            LoadDanhSachGiaoDich(tuNgay, denNgay, loaiHD);
        }

        private void XuatFileExcel()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel File (*.xls)|*.xls";
            sfd.FileName = "BaoCaoDoanhThu_" + DateTime.Now.ToString("ddMMyyyy_HHmm") + ".xls";

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    DataView view = (DataView)dgvDoanhThu.ItemsSource;
                    if (view == null || view.Count == 0)
                    {
                        MessageBox.Show("Không có dữ liệu trong bảng để xuất!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true)))
                    {
                        sw.WriteLine("Mã HD\tNgày lập\tLoại\tChi tiết\tĐơn giá\tSố lượng\tThành tiền");

                        foreach (DataRowView row in view)
                        {
                            string maHD = row["MaHD"].ToString();
                            string ngayLap = Convert.ToDateTime(row["NgayLapHD"]).ToString("dd/MM/yyyy HH:mm");
                            string loai = row["LoaiHoaDon"].ToString();

                            string chiTiet = row["ChiTiet"].ToString().Replace("\t", " ");

                            string donGia = row["DonGia"].ToString();
                            string soLuong = row["SoLuong"].ToString();
                            string thanhTien = row["ThanhTien"].ToString();

                            sw.WriteLine($"{maHD}\t{ngayLap}\t{loai}\t{chiTiet}\t{donGia}\t{soLuong}\t{thanhTien}");
                        }
                    }

                    MessageBox.Show("Đã xuất báo cáo thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
     
    }
}
