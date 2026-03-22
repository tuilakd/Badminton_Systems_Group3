using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
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
    public partial class Client : Window
    {
        ClientBUS bus = new ClientBUS();

        public Client()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            dataGrid.ItemsSource = bus.GetAll();
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            string maKH = txtMaKH.Text.Trim().ToUpper();
            string hoTen = txtHoTen.Text.Trim();
            string sdt = txtSDT.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(sdt, @"^\d{10}$"))
            {
                MessageBox.Show("SĐT phải là 10 chữ số!");
                return;
            }
            if (string.IsNullOrEmpty(maKH) ||
                string.IsNullOrEmpty(hoTen) ||
                string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin");
                return;
            }
            if (bus.Exists(maKH))
            {
                MessageBox.Show("Mã khách hàng đã tồn tại!");
                return;
            }
            ClientDTO kh = new ClientDTO
            {
                MaKH = maKH,
                HoTen = hoTen,
                SDT = sdt
            };
            bus.Add(kh);

            MessageBox.Show("Thêm thành công");
            ClearForm();
            LoadData();
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaKH.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                bus.Delete(txtMaKH.Text);

                MessageBox.Show("Xóa thành công");
                ClearForm();
                LoadData();
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaKH.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần sửa");
                return;
            }

            ClientDTO kh = new ClientDTO
            {
                MaKH = txtMaKH.Text,
                HoTen = txtHoTen.Text.Trim(),
                SDT = txtSDT.Text.Trim()
            };

            bus.Update(kh);

            MessageBox.Show("Cập nhật thành công");
            ClearForm();
            LoadData();
        }
        
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem == null) return;

            ClientDTO kh = (ClientDTO)dataGrid.SelectedItem;

            txtMaKH.Text = kh.MaKH;
            txtHoTen.Text = kh.HoTen;
            txtSDT.Text = kh.SDT;
        }
        void ClearForm()
        {
            txtMaKH.Text = "";
            txtHoTen.Text = "";
            txtSDT.Text = "";
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

        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Lấy từ khóa
                string keyword = txtTimKiem.Text.Trim();

                // [MẸO KIỂM TRA]: Bật hộp thoại này lên để xem nút có nhận lệnh không
                // Nếu bấm nút mà không hiện hộp thoại này -> Bạn làm sai Bước 1
                // MessageBox.Show("Bạn vừa tìm từ khóa: " + keyword); 

                // 2. Gọi BUS để tìm kiếm
                List<ClientDTO> ketQua = bus.Search(keyword);

                // 3. Cập nhật bảng (Dùng đúng tên dataGrid của bạn)
                dataGrid.ItemsSource = null; // Xóa dữ liệu cũ đi trước cho chắc
                dataGrid.ItemsSource = ketQua;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm: " + ex.Message);
            }
        }
    }
}
