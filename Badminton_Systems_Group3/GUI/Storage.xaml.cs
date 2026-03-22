using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Data;
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
    
    
    public partial class Storage : Window
    {
        WareHouseBUS bus = new WareHouseBUS();
        public Storage()
        {
            InitializeComponent();
            HienThiTatCa();
        }
        


        private void btnThongTin_Click(object sender, RoutedEventArgs e)
        {
            Badminton_Systems_Group3.GUI.Info thongTinWindow = new Badminton_Systems_Group3.GUI.Info();
            thongTinWindow.ShowDialog();

            btnThongTin.IsChecked = false;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Home window = new Home();
            window.Show();
            this.Close();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            BookingSchedule window = new BookingSchedule();
            window.Show();
            this.Close();
        }

        private void btnNhapKho_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSP.Text) || string.IsNullOrWhiteSpace(txtSoLuong.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            bool laSoLuongHopLe = int.TryParse(txtSoLuong.Text, out int soLuong);
            bool laDonGiaHopLe = decimal.TryParse(txtDonGia.Text, out decimal donGia);

            if (!laSoLuongHopLe || !laDonGiaHopLe)
            {
                MessageBox.Show("Dữ liệu số lượng/đơn giá không hợp lệ", "Lỗi định dạng", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var entry = new StockEntryDTO
            {
                MaSP = txtMaSP.Text.Trim(),
                DonGia = donGia,
                SoLuongNhap = soLuong,
                NgayNhap = dpNgayNhap.SelectedDate ?? DateTime.Now
            };

            string ketQua = bus.XuLyGiaoDich(entry, true);
            MessageBox.Show(ketQua);

            HienThiTatCa();
        }

        private void dgvKho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvKho.SelectedItem != null)
            {
                try
                {
                    DataRowView row = (DataRowView)dgvKho.SelectedItem;

                    txtMaSP.Text = row["MaSP"].ToString();
                    txtTenSP.Text = row["TenSP"].ToString();
                    txtDonGia.Text = row["DonGia"].ToString();

                    txtSoLuong.Clear();
                    txtSoLuong.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lấy dữ liệu hàng: " + ex.Message);
                }
            }
        }

        private StockEntryDTO LayThongTinTuForm()
        {
            return new StockEntryDTO
            {
                MaSP = txtMaSP.Text,
                DonGia = decimal.TryParse(txtDonGia.Text, out decimal gia) ? gia : 0,
                SoLuongNhap = int.TryParse(txtSoLuong.Text, out int sl) ? sl : 0,
                NgayNhap = dpNgayNhap.SelectedDate ?? DateTime.Now
            };
        }

        private void HienThiTatCa()
        {
            List<string> selectedList = new List<string>();

            if (chkReviveVang.IsChecked == true) selectedList.Add(chkReviveVang.Content.ToString());
            if (chkReviveTrang.IsChecked == true) selectedList.Add(chkReviveTrang.Content.ToString());
            if (chkLavie.IsChecked == true) selectedList.Add(chkLavie.Content.ToString());
            if (chkQuanCanHBT.IsChecked == true) selectedList.Add(chkQuanCanHBT.Content.ToString());
            if (chkCauThanhCong.IsChecked == true) selectedList.Add(chkCauThanhCong.Content.ToString());
            if (chkCauXSMash.IsChecked == true) selectedList.Add(chkCauXSMash.Content.ToString());

            dgvKho.ItemsSource = bus.LayKho(selectedList).DefaultView;
        }

        private void OnCheckboxChanged(object sender, RoutedEventArgs e)
        {
            HienThiTatCa();
        }

        private void btnKiemKe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataView view = (DataView)dgvKho.ItemsSource;

                if (view == null || view.Count == 0)
                {
                    MessageBox.Show("Kho hiện đang trống hoặc chưa có dữ liệu hiển thị!", "Kiểm kê", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                int tongSoMatHang = view.Count;
                int tongSoLuongTon = 0;
                decimal tongGiaTriKho = 0;

                foreach (DataRowView row in view)
                {
                    int tonKho = row["SoLuongTon"] != DBNull.Value ? Convert.ToInt32(row["SoLuongTon"]) : 0;
                    decimal donGia = row["DonGia"] != DBNull.Value ? Convert.ToDecimal(row["DonGia"]) : 0;

                    tongSoLuongTon += tonKho;
                    tongGiaTriKho += (tonKho * donGia);
                }

                string thongBao = $"KẾT QUẢ KIỂM KÊ KHO ({DateTime.Now.ToString("dd/MM/yyyy HH:mm")}):\n\n" +
                                  $"- Tổng số loại sản phẩm: {tongSoMatHang}\n" +
                                  $"- Tổng số lượng sản phẩm tồn: {tongSoLuongTon}\n" +
                                  $"- Ước tính tổng giá trị kho: {tongGiaTriKho:N0} VNĐ";

                MessageBox.Show(thongBao, "Báo cáo Kiểm Kê Kho", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi kiểm kê: Vui lòng kiểm tra lại tên cột trong CSDL.\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void btnTrangChu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnThongTin_Checked(object sender, RoutedEventArgs e)
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
    }
}
