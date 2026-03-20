using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DTO;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Badminton_Systems_Group3.GUI
{
    public partial class Booking : Window
    {
        private string maSanDangChon = "";
        private readonly BookingBUS bus = new BookingBUS();

        public Booking()
        {
            InitializeComponent();
            DisableForm();
            LoadSanTuDatabase();
        }

        public void LoadSanTuDatabase()
        {
            try
            {
                var db = new Badminton_Systems_Group3.Database.DatabaseHelper();
                DataTable dt = db.ExecuteQuery("SELECT MaSan, TrangThai FROM san");

                foreach (DataRow row in dt.Rows)
                {
                    string ma = row["MaSan"]?.ToString() ?? "";
                    string tt = row["TrangThai"]?.ToString() ?? "";
                    bool isBusy = (tt == "Đã đặt");
                    UpdateCourtStatusUI(ma, isBusy);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu sân: " + ex.Message);
            }
        }

        private void UpdateCourtStatusUI(string maSan, bool isBusy)
        {
            if (string.IsNullOrEmpty(maSan)) return;

            // Tìm TextBlock trạng thái (VD: txtStatus_SB0001)
            var txt = this.FindName("txtStatus_" + maSan) as TextBlock;
            // Tìm Button đặt sân (Dựa vào x:Name bạn đặt trong XAML, VD: btn_SB0001)
            var btn = this.FindName("btn_" + maSan) as Button;

            if (txt != null)
            {
                txt.Text = isBusy ? "ĐÃ ĐẶT" : "TRỐNG";
                txt.Foreground = isBusy ? Brushes.Red : Brushes.Green;
            }

            if (btn != null)
            {
                if (isBusy)
                {
                    btn.Content = "THANH TOÁN";
                    btn.Background = Brushes.Red;
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    btn.Content = "Đặt sân";
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D8C57")); // Màu xanh cũ
                    btn.Foreground = Brushes.White;
                }
            }
        }

        private void LocSan()
        {
            // Lấy giờ từ SelectedItem để đảm bảo chính xác nhất
            string strStart = (cboGioBD.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioBD.Text;
            string strEnd = (cboGioKT.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioKT.Text;

            if (dpNgayDat.SelectedDate == null || string.IsNullOrEmpty(strStart) || string.IsNullOrEmpty(strEnd))
                return;

            try
            {
                if (DateTime.TryParse(strStart, out DateTime start) && DateTime.TryParse(strEnd, out DateTime end))
                {
                    if (end <= start) return;

                    var dal = new Badminton_Systems_Group3.DAL.BookingDAL();
                    // Truy vấn những sân đã có người đặt trong khoảng start - end của ngày đã chọn
                    DataTable dtDaDat = dal.GetSanDaDat(dpNgayDat.SelectedDate.Value, start, end);
                    var busyLanes = dtDaDat.AsEnumerable().Select(r => r["MaSan"].ToString()).ToList();

                    // QUAN TRỌNG: Duyệt qua tất cả các sân (từ 1 đến 8) để cập nhật lại trạng thái
                    string[] tatCaMaSan = { "SB0001", "SB0002", "SB0003", "SB0004", "SB0005", "SB0006", "SB0007", "SB0008" };

                    foreach (string ma in tatCaMaSan)
                    {
                        bool isBusy = busyLanes.Contains(ma);
                        UpdateCourtStatusUI(ma, isBusy);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi Lọc Sân: " + ex.Message);
            }
        }

        private decimal TinhToanThanhTien(DateTime start, DateTime end)
        {
            TimeSpan duration = end - start;
            double hours = duration.TotalHours;
            return hours > 0 ? (decimal)hours * 120000 : 0;
        }

        private void CapNhatTienGiaoDien()
        {
            // Dùng SelectedItem để lấy dữ liệu "nóng" ngay khi vừa click
            string strStart = (cboGioBD.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioBD.Text;
            string strEnd = (cboGioKT.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioKT.Text;

            if (DateTime.TryParse(strStart, out DateTime start) &&
                DateTime.TryParse(strEnd, out DateTime end))
            {
                if (end > start)
                {
                    decimal tongTien = TinhToanThanhTien(start, end);
                    double soGio = (end - start).TotalHours;

                    lblTamTinh.Text = tongTien.ToString("N0") + " VNĐ";
                    lblTongGio.Text = soGio.ToString("0.##") + " giờ";
                }
                else
                {
                    lblTamTinh.Text = "0 VNĐ";
                    lblTongGio.Text = "0 giờ";
                }
            }
        }

        private void btnXacNhan_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(maSanDangChon))
            {
                MessageBox.Show("Vui lòng click chọn một sân trên bản đồ!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenKH.Text) || string.IsNullOrWhiteSpace(txtSDT.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên và SĐT khách hàng!");
                return;
            }

            try
            {
                string strStart = (cboGioBD.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioBD.Text;
                string strEnd = (cboGioKT.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioKT.Text;

                if (!DateTime.TryParse(strStart, out DateTime start) ||
                    !DateTime.TryParse(strEnd, out DateTime end)) return;

                // Rút ngắn mã để tránh lỗi CHECK constraint (image_9b5198.jpg)
                string last4SDT = txtSDT.Text.Length >= 4 ? txtSDT.Text.Substring(txtSDT.Text.Length - 4) : txtSDT.Text;
                string maKH = "KH" + last4SDT.PadLeft(4, '0');
                string maDS = "DS" + DateTime.Now.ToString("mm") + DateTime.Now.ToString("ss");

                BookingDTO booking = new BookingDTO
                {
                    MaDatSan = maDS,
                    MaKH = maKH,
                    MaSan = maSanDangChon,
                    TenKhachHang = txtTenKH.Text,
                    SDT = txtSDT.Text,
                    NgayDat = dpNgayDat.SelectedDate ?? DateTime.Today,
                    GioBatDau = start,
                    GioKetThuc = end,
                    GiaThue = 120000,
                    ThanhTien = TinhToanThanhTien(start, end)
                };

                var dal = new Badminton_Systems_Group3.DAL.BookingDAL();
                // Lưu khách hàng trước để tránh lỗi Foreign Key (image_9cae1e.jpg)
                dal.InsertKhachHang(booking.MaKH, booking.TenKhachHang, booking.SDT);

                string result = bus.ThucHienDatSan(booking);
                MessageBox.Show($"{result}\nTổng thanh toán: {booking.ThanhTien:N0} VNĐ");

                LoadSanTuDatabase();
                ResetForm();
                DisableForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        private void ResetForm()
        {
            txtTenKH.Clear();
            txtSDT.Clear();
            maSanDangChon = "";
            lblTamTinh.Text = "0 VNĐ";
            lblTongGio.Text = "0 giờ";
        }

        private void cboGioBD_SelectionChanged(object sender, SelectionChangedEventArgs e) { CapNhatTienGiaoDien(); LocSan(); }
        private void cboGioKT_SelectionChanged(object sender, SelectionChangedEventArgs e) { CapNhatTienGiaoDien(); LocSan(); }
        private void dpNgayDat_SelectedDateChanged(object sender, SelectionChangedEventArgs e) { LocSan(); CapNhatTienGiaoDien(); }

        private void SB0001_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                maSanDangChon = btn.Tag.ToString();
                EnableForm();
                txtTenKH.Focus();
            }
        }

        private void DisableForm() { txtTenKH.IsEnabled = false; txtSDT.IsEnabled = false; }
        private void EnableForm() { txtTenKH.IsEnabled = true; txtSDT.IsEnabled = true; }
    }
}