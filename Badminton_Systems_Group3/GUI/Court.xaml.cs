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
    /// <summary>
    /// Interaction logic for Court.xaml
    /// </summary>
    public partial class Court : Window
    {
        private CourtBUS bus = new CourtBUS();
        public Court()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            dgSanBai.ItemsSource = bus.GetAll();
        }
        private void btnThongTin_Click(object sender, RoutedEventArgs e)
        {
            Badminton_Systems_Group3.GUI.Info thongTinWindow = new Badminton_Systems_Group3.GUI.Info();
            thongTinWindow.ShowDialog();

            btnThongTin.IsChecked = false;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            BookingSchedule window = new BookingSchedule();
            window.Show();
            this.Close();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            Home window = new Home();
            window.Show();
            this.Close();
        }

        private void dgSanBai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSanBai.SelectedItem is CourtDTO selectedSB)
            {
                txtMaSan.Text = selectedSB.MaSan;
                txtTenSan.Text = selectedSB.TenSan;
                cboTrangThai.Text = selectedSB.TrangThai; 
                txtGiaThue.Text = selectedSB.GiaThue.ToString();

                txtMaSan.IsEnabled = false;
            }
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSan.Text) || string.IsNullOrEmpty(txtTenSan.Text) || string.IsNullOrEmpty(txtGiaThue.Text))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtGiaThue.Text, out double giaThue))
            {
                MessageBox.Show("Dữ liệu không hợp lệ, vui lòng nhập lại", "Lỗi đỏ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CourtDTO sb = new CourtDTO()
            {
                MaSan = txtMaSan.Text.Trim(),
                TenSan = txtTenSan.Text.Trim(),
                TrangThai = cboTrangThai.Text.Trim(),
                GiaThue = giaThue
            };

            if (bus.Insert(sb))
            {
                MessageBox.Show("Thêm thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Lỗi khi thêm. Có thể Mã Sân đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            string maSan = txtMaSan.Text.Trim();
            string trangThai = cboTrangThai.Text.Trim();

            if (string.IsNullOrEmpty(maSan))
            {
                MessageBox.Show("Vui lòng chọn sân cần xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (trangThai.ToLower().Contains("đang hoạt động"))
            {
                MessageBox.Show("Sân đang hoạt động, không thể xóa lúc này", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa sân này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (bus.Delete(maSan))
                {
                    MessageBox.Show("Đã xóa thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                    ClearForm();
                }
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSan.Text) || string.IsNullOrEmpty(txtTenSan.Text))
            {
                MessageBox.Show("Vui lòng chọn sân cần sửa và điền đủ thông tin", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtGiaThue.Text, out double giaThue))
            {
                MessageBox.Show("Dữ liệu không hợp lệ, vui lòng nhập lại", "Lỗi đỏ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CourtDTO sb = new CourtDTO()
            {
                MaSan = txtMaSan.Text.Trim(),
                TenSan = txtTenSan.Text.Trim(),
                TrangThai = cboTrangThai.Text.Trim(),
                GiaThue = giaThue
            };

            if (bus.Update(sb))
            {
                MessageBox.Show("Cập nhật thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ClearForm();
            }
        }
        private void ClearForm()
        {
            txtMaSan.Clear();
            txtTenSan.Clear();
            txtGiaThue.Clear();
            cboTrangThai.SelectedIndex = -1; 
            txtMaSan.IsEnabled = true;       
        }

        private void cboHienThi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dgSanBai != null && cboHienThi != null && cboHienThi.SelectedItem != null)
    {
                ComboBoxItem item = (ComboBoxItem)cboHienThi.SelectedItem;
                string filter = item.Content.ToString();

                DataTable dt = bus.LayDanhSachSan(filter);

                dgSanBai.ItemsSource = dt.DefaultView;
            }
        }
    }
}
