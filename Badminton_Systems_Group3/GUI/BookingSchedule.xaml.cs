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
            if (dgLichDat.SelectedItem == null)
            {
                MessageBox.Show("Chọn lịch cần hủy!");
                return;
            }

            DataRowView row = (DataRowView)dgLichDat.SelectedItem;
            string maDatSan = row["MaDatSan"].ToString();

            var confirm = MessageBox.Show("Bạn chắc chắn hủy?", "Xác nhận", MessageBoxButton.YesNo);

            if (confirm == MessageBoxResult.Yes)
            {
                var kq = bus.HuyLich(maDatSan);
                MessageBox.Show(kq.message);

                if (kq.success)
                    LoadData();
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
    }
}