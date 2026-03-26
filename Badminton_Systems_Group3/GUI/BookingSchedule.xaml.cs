using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

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

        private void ThucHienTimKiem()
        {
            try
            {
                string trangThai = "Tất cả";
                if (cbTrangThai.SelectedItem is ComboBoxItem item)
                {
                    trangThai = item.Content?.ToString() ?? "Tất cả";
                }

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

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThucHienTimKiem();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgLichDat.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn lịch đặt sân cần hủy!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRowView row = (DataRowView)dgLichDat.SelectedItem;

            string trangThai = row["TrangThai"].ToString();

            if (trangThai == "Đã thanh toán")
            {
                MessageBox.Show("Không thể hủy lịch đặt sân đã thanh toán!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            string maDatSan = row["MaDatSan"].ToString();
            var confirm = MessageBox.Show($" bạn chắc chắn muốn hủy lịch đặt {maDatSan}?", "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                var kq = bus.HuyLich(maDatSan);

                if (kq.Item1)
                {
                    MessageBox.Show("Hủy lịch thành công!", "Thông báo");
                    LoadData(); 
                }
                else
                {
                    MessageBox.Show("Lỗi khi hủy lịch: " + kq.Item2, "Lỗi");
                }
            }
        }

        private void dgLichDat_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                // Lấy dòng dữ liệu đang sửa
                DataRowView row = (DataRowView)e.Row.Item;

                string maDatSan = row["MaDatSan"].ToString();
                string maSan = row["MaSan"].ToString();

                DateTime ngay = Convert.ToDateTime(row["NgayDat"]);
                TimeSpan bd = TimeSpan.Parse(row["GioBD"].ToString());
                TimeSpan kt = TimeSpan.Parse(row["GioKT"].ToString());

                CourtDAL courtDAL = new CourtDAL();
                var court = courtDAL.GetByMaSan(maSan);

                decimal giaThue = court != null
                    ? Convert.ToDecimal(court.GiaThue)
                    : 0;
                var (success, message) = bus.UpdateBooking(maDatSan, ngay, bd, kt, maSan, giaThue);

                if (!success)
                {
                    MessageBox.Show(message);
                    LoadData();
                }
                else
                {
                    
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa: " + ex.Message);
                LoadData(); 
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
            string maSan = row["MaSan"].ToString(); 
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