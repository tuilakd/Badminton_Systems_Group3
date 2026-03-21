using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using Badminton_Systems_Group3.BUS;
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
        private string maDatSanDangChon = "";

        private readonly BookingDAL dal = new BookingDAL();
        private readonly BookingBUS bus = new BookingBUS();

        private readonly string[] dsSan =
        {
            "SB0001","SB0002","SB0003","SB0004",
            "SB0005","SB0006","SB0007","SB0008"
        };

        public Booking()
        {
            InitializeComponent();
            dpNgayDat.SelectedDate = DateTime.Today;
            LoadSanMacDinh();
            DisableForm();
        }

        private void UpdateUI(string maSan, bool isBusy)
        {
            var txt = FindName("txtStatus_" + maSan) as TextBlock;
            var btn = FindName("btn_" + maSan) as Button;

            if (txt != null)
            {
                txt.Text = isBusy ? "ĐÃ ĐẶT" : "TRỐNG";
                txt.Foreground = isBusy ? Brushes.Red : Brushes.Green;
            }

            if (btn != null)
            {
                btn.Content = isBusy ? "THANH TOÁN" : "Đặt sân";
                btn.Background = isBusy ? Brushes.Red : Brushes.Green;
            }
        }

        private void LoadSanMacDinh()
        {
            foreach (var ma in dsSan)
                UpdateUI(ma, false);
        }

        private void LocSan()
        {
            if (dpNgayDat.SelectedDate == null) return;

            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT))
            {
                LoadSanMacDinh();
                return;
            }

            DataTable dt = bus.GetSanDaDat(dpNgayDat.SelectedDate.Value, gioBD, gioKT);
            var busy = dt.AsEnumerable().Select(r => r["MaSan"]?.ToString() ?? "").ToList();

            foreach (var ma in dsSan)
                UpdateUI(ma, busy.Contains(ma));
        }

        private void SB0001_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag == null) return;

            maSanDangChon = btn.Tag.ToString()!;
            DateTime ngay = dpNgayDat.SelectedDate ?? DateTime.Today;

            bool coBooking = bus.KiemTraSanDaDat(maSanDangChon, ngay);

            if (coBooking)
            {
                LoadThongTin(); 
            }
            else
            {
                ResetForm();
                EnableForm();
                btnXacNhan.Content = "ĐẶT SÂN";
                CapNhatTien();
            }
        }

        private void LoadThongTin()
        {
            if (string.IsNullOrEmpty(maSanDangChon) || dpNgayDat.SelectedDate == null)
                return;

            DataRow dr = dal.GetThongTinKhachDatSanChuaThanhToan(maSanDangChon, dpNgayDat.SelectedDate.Value);
            if (dr == null)
            {
                MessageBox.Show("Không tìm thấy booking chưa thanh toán!");
                return;
            }

            maDatSanDangChon = dr["MaDatSan"].ToString();
            txtTenKH.Text = dr["HoTen"].ToString();
            txtSDT.Text = dr["SDT"].ToString();

            TimeSpan gioBD = (TimeSpan)dr["GioBD"];
            TimeSpan gioKT = (TimeSpan)dr["GioKT"];

            SetComboBoxTime(cboGioBD, gioBD);
            SetComboBoxTime(cboGioKT, gioKT);

            lblTongGio.Text = $"{(gioKT - gioBD).TotalHours} giờ";
            decimal thanhTien = Convert.ToDecimal(dr["ThanhTien"]);
            lblTamTinh.Text = string.Format("{0:N0} VNĐ", thanhTien);

            btnXacNhan.Content = "THANH TOÁN";
            DisableForm();
        }

        private void CapNhatTien()
        {
            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT))
            {
                lblTongGio.Text = "0 giờ";
                lblTamTinh.Text = "0 VNĐ";
                return;
            }

            double gio = (gioKT - gioBD).TotalHours;
            lblTongGio.Text = $"{gio} giờ";

            decimal tien = (decimal)gio * 120000;
            lblTamTinh.Text = string.Format("{0:N0} VNĐ", tien);
        }

        private void DatSan()
        {
            if (dpNgayDat.SelectedDate == null || string.IsNullOrEmpty(maSanDangChon))
            {
                MessageBox.Show("Chọn sân và ngày trước!");
                return;
            }

            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT))
            {
                MessageBox.Show("Chọn giờ hợp lệ!");
                return;
            }

            BookingDTO booking = new BookingDTO
            {
                MaSan = maSanDangChon,
                TenKhachHang = txtTenKH.Text,
                SDT = txtSDT.Text,
                NgayDat = dpNgayDat.SelectedDate.Value,
                GioBatDau = gioBD,
                GioKetThuc = gioKT
            };

            var result = bus.ThucHienDatSan(booking);
            MessageBox.Show(result.message);

            if (result.success)
            {
                ResetForm();
                LocSan();
            }
        }

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(maSanDangChon))
            {
                MessageBox.Show("Chọn sân trước!");
                return;
            }

            string content = btnXacNhan.Content.ToString() ?? "";

            if (content == "THANH TOÁN")
            {
                var drTemp = dal.GetThongTinKhachDatSanChuaThanhToan(maSanDangChon, dpNgayDat.SelectedDate ?? DateTime.Today);
                if (drTemp == null)
                {
                    MessageBox.Show("Không tìm thấy booking chưa thanh toán!");
                    return;
                }

                maDatSanDangChon = drTemp["MaDatSan"].ToString();
                string sdt = drTemp["SDT"].ToString();
                decimal thanhTien = Convert.ToDecimal(drTemp["ThanhTien"]);

                var result = bus.ThanhToan(maDatSanDangChon, sdt, thanhTien);
                MessageBox.Show(result.message);

                if (result.success)
                {
                    ResetForm();
                    LocSan();
                }
            }
            else if (content == "ĐẶT SÂN")
            {
                CapNhatTien();
                DatSan();
            }
        }

        private void ResetForm()
        {
            txtTenKH.Clear();
            txtSDT.Clear();
            lblTamTinh.Text = "0 VNĐ";
            lblTongGio.Text = "0 giờ";
            maDatSanDangChon = "";
        }

        private void EnableForm()
        {
            txtTenKH.IsEnabled = true;
            txtSDT.IsEnabled = true;
        }

        private void DisableForm()
        {
            txtTenKH.IsEnabled = false;
            txtSDT.IsEnabled = false;
        }

        private void cboGioBD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CapNhatTien();
            LocSan();
        }

        private void cboGioKT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CapNhatTien();
            LocSan();
        }

        private void dpNgayDat_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LocSan();
        }

        private bool TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT)
        {
            gioBD = TimeSpan.Zero;
            gioKT = TimeSpan.Zero;

            string strStart = (cboGioBD.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string strEnd = (cboGioKT.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            return TimeSpan.TryParse(strStart, out gioBD) &&
                   TimeSpan.TryParse(strEnd, out gioKT) &&
                   gioKT > gioBD;
        }

        private void SetComboBoxTime(ComboBox combo, TimeSpan time)
        {
            var item = combo.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(i => TimeSpan.Parse(i.Content.ToString()) == time);
            if (item != null)
                combo.SelectedItem = item;
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
    }
}