using System;
using System.Data;
using System.Windows;
using Badminton_Systems_Group3.BUS; // Cần thiết để dùng BookingBUS

namespace Badminton_Systems_Group3.GUI
{
    public partial class BookingSchedule : Window
    {
        // Khai báo biến bus ở cấp độ lớp để tất cả các hàm đều dùng được
        private readonly BookingBUS bus = new BookingBUS();

        public BookingSchedule()
        {
            InitializeComponent();
            LoadData(); // Gọi hàm nạp dữ liệu ngay khi mở cửa sổ
        }

        private void LoadData()
        {
            try
            {
                // Gọi hàm lấy toàn bộ lịch đặt từ lớp BUS
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
    }
}