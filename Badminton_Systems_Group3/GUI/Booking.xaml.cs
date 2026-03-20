using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DTO;
using Badminton_Systems_Group3.DAL; // Thêm để gọi DAL
using System;
using System.Data;
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
        private readonly BookingDAL dal = new BookingDAL(); // Khai báo dùng chung

        public Booking()
        {
            InitializeComponent();
            DisableForm();
            LoadSanTuDatabase();
        }

        // --- CÁC HÀM CẬP NHẬT GIAO DIỆN (Giữ nguyên logic cũ của bạn) ---
        public void LoadSanTuDatabase()
        {
            try
            {
                var db = new Badminton_Systems_Group3.Database.DatabaseHelper();
                DataTable dt = db.ExecuteQuery("SELECT MaSan, TrangThai FROM san");
                foreach (DataRow row in dt.Rows)
                {
                    string ma = row["MaSan"]?.ToString() ?? "";
                    bool isBusy = (row["TrangThai"]?.ToString() == "Đã đặt");
                    UpdateCourtStatusUI(ma, isBusy);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void UpdateCourtStatusUI(string maSan, bool isBusy)
        {
            var txt = this.FindName("txtStatus_" + maSan) as TextBlock;
            var btn = this.FindName("btn_" + maSan) as Button; // Đảm bảo XAML đặt tên là btn_SB0001

            if (txt != null)
            {
                txt.Text = isBusy ? "ĐÃ ĐẶT" : "TRỐNG";
                txt.Foreground = isBusy ? Brushes.Red : Brushes.Green;
            }
            if (btn != null)
            {
                btn.Content = isBusy ? "THANH TOÁN" : "Đặt sân";
                btn.Background = isBusy ? Brushes.Red : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D8C57"));
            }
        }

        private void LocSan()
        {
            string strStart = (cboGioBD.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioBD.Text;
            string strEnd = (cboGioKT.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioKT.Text;

            if (dpNgayDat.SelectedDate == null || string.IsNullOrEmpty(strStart) || string.IsNullOrEmpty(strEnd)) return;

            if (DateTime.TryParse(strStart, out DateTime start) && DateTime.TryParse(strEnd, out DateTime end))
            {
                if (end <= start) return;
                DataTable dtDaDat = dal.GetSanDaDat(dpNgayDat.SelectedDate.Value, start, end);
                var busyLanes = dtDaDat.AsEnumerable().Select(r => r["MaSan"].ToString()).ToList();
                string[] dsMaSan = { "SB0001", "SB0002", "SB0003", "SB0004", "SB0005", "SB0006", "SB0007", "SB0008" };
                foreach (string ma in dsMaSan) UpdateCourtStatusUI(ma, busyLanes.Contains(ma));
            }
        }

        private void HienThiThongTinKhachDaDat(string maSan)
        {
            try
            {
                string strStart = (cboGioBD.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioBD.Text;
                string strEnd = (cboGioKT.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cboGioKT.Text;

                if (DateTime.TryParse(strStart, out DateTime start) && DateTime.TryParse(strEnd, out DateTime end))
                {
                    DataRow dr = dal.GetThongTinKhachDatSan(maSan, dpNgayDat.SelectedDate ?? DateTime.Today, start, end);

                    if (dr != null)
                    {
                        txtTenKH.Text = dr["HoTen"].ToString();
                        txtSDT.Text = dr["SDT"].ToString();
                        lblTamTinh.Text = string.Format("{0:N0} VNĐ", dr["ThanhTien"]);

                        // Đổi nút Xác nhận thành nút Thanh Toán để làm bước 2.4 trong đặc tả
                        btnXacNhan.Content = "XÁC NHẬN THANH TOÁN";
                        btnXacNhan.Background = Brushes.OrangeRed;

                        DisableForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị: " + ex.Message);
            }
        }

        private void SB0001_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                maSanDangChon = btn.Tag.ToString();
                if (btn.Content.ToString() == "THANH TOÁN")
                {
                    HienThiThongTinKhachDaDat(maSanDangChon);
                }
                else
                {
                    ResetForm();
                    EnableForm();
                    txtTenKH.Focus();
                }
            }
        }

        // --- CÁC SỰ KIỆN KHÁC ---
        private void cboGioBD_SelectionChanged(object sender, SelectionChangedEventArgs e) { ResetForm(); LocSan(); }
        private void cboGioKT_SelectionChanged(object sender, SelectionChangedEventArgs e) { ResetForm(); LocSan(); }
        private void dpNgayDat_SelectedDateChanged(object sender, SelectionChangedEventArgs e) { LocSan(); }

        private void ResetForm()
        {
            txtTenKH.Clear(); txtSDT.Clear(); maSanDangChon = "";
            lblTamTinh.Text = "0 VNĐ"; lblTongGio.Text = "0 giờ";
            EnableForm();
        }

        private void DisableForm() { txtTenKH.IsEnabled = false; txtSDT.IsEnabled = false; }
        private void EnableForm() { txtTenKH.IsEnabled = true; txtSDT.IsEnabled = true; }

        // Nút xác nhận đặt sân (Giữ nguyên logic tạo mã ngắn gọn của bạn)
        private void btnXacNhan_Click_1(object sender, RoutedEventArgs e) { /* ... Logic đặt sân cũ của bạn ... */ }
    }
}