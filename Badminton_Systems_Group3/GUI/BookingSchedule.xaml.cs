using Badminton_Systems_Group3.BUS;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace Badminton_Systems_Group3.GUI
{
    public partial class BookingSchedule : Window
    {
        private readonly BookingBUS bus = new BookingBUS();

        public BookingSchedule()
        {
            InitializeComponent();
            LoadData(); 
        }

        private void LoadData()
        {
            try
            {
                
                DataTable dt = bus.LayLichDatSanFull();

                if (dt != null && dgLichDat != null)
                {
                    dgLichDat.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp dữ liệu: " + ex.Message);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Home window = new Home();
            window.Show();
            this.Close();
        }

        private void btnThongTin_Click(object sender, RoutedEventArgs e)
        {
            Badminton_Systems_Group3.GUI.Info thongTinWindow = new Badminton_Systems_Group3.GUI.Info();
            thongTinWindow.ShowDialog();

            if (btnThongTin != null)
            {
                btnThongTin.IsChecked = false;
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string trangThai = (cbTrangThai.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Tất cả";
                string keyword = txtTen.Text.Trim(); 
                DateTime? ngay = dpNgay.SelectedDate;

                DataTable dt = bus.SearchBooking(trangThai, keyword, ngay);

                dgLichDat.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra xem người dùng đã chọn dòng nào trên DataGrid chưa
            if (dgLichDat.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn lịch đặt sân cần hủy!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Lấy dòng dữ liệu đang được chọn
            DataRowView row = (DataRowView)dgLichDat.SelectedItem;

            // 3. LẤY TRẠNG THÁI CỦA LỊCH ĐẶT (Quan trọng)
            // Lưu ý: "TrangThai" phải khớp với tên cột trong DataTable của bạn
            string trangThai = row["TrangThai"].ToString();

            // 4. KIỂM TRA LOGIC: Nếu đã thanh toán thì không cho xóa
            if (trangThai == "Đã thanh toán")
            {
                MessageBox.Show("Không thể hủy lịch đặt sân đã thanh toán!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Dừng hàm tại đây, không chạy xuống phần xóa bên dưới
            }

            // 5. Nếu chưa thanh toán, tiến hành xác nhận và xóa như cũ
            string maDatSan = row["MaDatSan"].ToString();
            var confirm = MessageBox.Show($" bạn chắc chắn muốn hủy lịch đặt {maDatSan}?", "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                var kq = bus.HuyLich(maDatSan);

                if (kq.success)
                {
                    MessageBox.Show("Hủy lịch thành công!", "Thông báo");
                    LoadData(); // Load lại bảng để cập nhật dữ liệu mới
                }
                else
                {
                    MessageBox.Show("Lỗi khi hủy lịch: " + kq.message, "Lỗi");
                }
            }
        }

        private void dgLichDat_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)e.Row.Item;

                string maDatSan = row["MaDatSan"].ToString();
                string maSan = row["MaSan"].ToString();

                DateTime ngay = Convert.ToDateTime(row["NgayDat"]);
                TimeSpan bd = TimeSpan.Parse(row["GioBD"].ToString());
                TimeSpan kt = TimeSpan.Parse(row["GioKT"].ToString());

                var kq = bus.UpdateBooking(maDatSan, ngay, bd, kt, maSan);

                if (!kq.success)
                {
                    MessageBox.Show(kq.message);
                    LoadData(); 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa: " + ex.Message);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgLichDat.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một lịch đặt trên bảng để thay đổi!", "Nhắc nhở", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Data.DataRowView row = (System.Data.DataRowView)dgLichDat.SelectedItem;

            string maDS = row["MaDatSan"].ToString();
            string maSan = row["MaSan"].ToString(); // LẤY THÊM MÃ SÂN Ở ĐÂY
            DateTime ngayCu = Convert.ToDateTime(row["NgayDat"]);
            TimeSpan gioBDCu = TimeSpan.Parse(row["GioBD"].ToString());
            TimeSpan gioKTCu = TimeSpan.Parse(row["GioKT"].ToString());

            ChangeBooking popup = new ChangeBooking (maDS, maSan, ngayCu, gioBDCu, gioKTCu);

            if (popup.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void cbTrangThai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (btnSearch != null)
            {
                BtnSearch_Click(sender, e);
            }
        }

        private void txtTen_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnSearch_Click(sender, e);

                // (Tùy chọn) Ngăn tiếng "beep" mặc định của Windows khi nhấn Enter trong TextBox
                e.Handled = true;
            }
        }

        private void dpNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (btnSearch != null)
            {
                BtnSearch_Click(sender, e);
            }
        }
    }
}