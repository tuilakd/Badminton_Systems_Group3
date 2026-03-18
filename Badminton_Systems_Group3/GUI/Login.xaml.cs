using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DAL;
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
    public partial class Login : Window
    {
        private UserBUS userBUS = new UserBUS();

        public Login()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            UserDTO loginUser = new UserDTO(username, password);

            if (userBUS.Login(loginUser))
            {
                MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                Home homeWindow = new Home();
                homeWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập, mật khẩu hoặc bạn chưa nhập đủ thông tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
