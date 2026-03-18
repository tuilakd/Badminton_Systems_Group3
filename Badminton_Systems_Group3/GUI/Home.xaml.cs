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
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
        }

      

       

        private void btnKho_Click(object sender, RoutedEventArgs e)
        {
            Storage window = new Storage();
            window.Show();
            this.Close();
        }

        private void btnDatSan_Click(object sender, RoutedEventArgs e)
        {
            Booking window = new Booking();
            window.Show();
            this.Close();
        }

        private void btnSanBai_Click(object sender, RoutedEventArgs e)
        {
            Court window = new Court();
            window.Show();
            this.Close();
        }

        private void btnDoanhThu_Click(object sender, RoutedEventArgs e)
        {
            Revenue window = new Revenue();
            window.Show();
            this.Close();
        }

        private void btnKhachHang_Click(object sender, RoutedEventArgs e)
        {
            Client window = new Client();
            window.Show();
            this.Close();
        }

        private void btnBanHang_Click(object sender, RoutedEventArgs e)
        {
            Sales window = new Sales();
            window.Show();
            this.Close();
        }

        private void btnTrangChu_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnLichDat_Checked(object sender, RoutedEventArgs e)
        {
            BookingSchedule window = new BookingSchedule();
            window.Show();
            this.Close();
        }

        private void btnThongTin_Checked(object sender, RoutedEventArgs e)
        {
            string appInfo = "Phần mềm Quản lý Sân Cầu Lông GR3\n" +
                             "Phiên bản: 1.0\n" +
                             "Nhà phát triển: Nhóm 3\n" +
                             "-----------------------------------\n" +
                             "Bạn có muốn ĐĂNG XUẤT khỏi hệ thống không?";

            MessageBoxResult result = MessageBox.Show(appInfo, "Thông tin & Đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                Login loginWindow = new Login();
                loginWindow.Show();

                this.Close();
            }
        }
    }
}
