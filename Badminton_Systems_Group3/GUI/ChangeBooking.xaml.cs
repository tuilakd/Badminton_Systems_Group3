using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DAL;
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
    public partial class ChangeBooking : Window
    {
        private string maDatSan;
        private string maSan;
        private BookingBUS bus = new BookingBUS();

        public ChangeBooking(string maDS, string maSan, DateTime ngayCu, TimeSpan gioBDCu, TimeSpan gioKTCu)
        {
            InitializeComponent();
            this.maDatSan = maDS;
            this.maSan = maSan;

            dpNgayMoi.SelectedDate = ngayCu;
            SetComboBoxTime(cboGioBD, gioBDCu);
            SetComboBoxTime(cboGioKT, gioKTCu);
        }

        private void SetComboBoxTime(ComboBox combo, TimeSpan time)
        {
            if (combo == null || combo.Items == null) return;

            foreach (ComboBoxItem item in combo.Items)
            {
                if (TimeSpan.TryParse(item.Content.ToString(), out TimeSpan itemTime))
                {
                    if (itemTime == time)
                    {
                        combo.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (dpNgayMoi.SelectedDate == null || cboGioBD.SelectedItem == null || cboGioKT.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ Ngày và Giờ mới!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeSpan gioBD = TimeSpan.Parse(((ComboBoxItem)cboGioBD.SelectedItem).Content.ToString());
            TimeSpan gioKT = TimeSpan.Parse(((ComboBoxItem)cboGioKT.SelectedItem).Content.ToString());

            if (gioKT <= gioBD)
            {
                MessageBox.Show("Giờ kết thúc phải lớn hơn giờ bắt đầu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CourtDAL courtDAL = new CourtDAL();
            var court = courtDAL.GetByMaSan(maSan);

            decimal gia = court != null ? Convert.ToDecimal(court.GiaThue) : 0;

            var (success, message) = bus.UpdateBooking(
                maDatSan,
                dpNgayMoi.SelectedDate.Value,
                gioBD,
                gioKT,
                maSan,
                gia
            );
            if (success)
            {
                MessageBox.Show(message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Báo lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}