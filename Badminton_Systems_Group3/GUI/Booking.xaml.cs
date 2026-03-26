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
        private readonly BookingBUS bus = new BookingBUS();
        private readonly CourtDAL courtDAL = new CourtDAL();
        private readonly BookingDAL bookingDAL = new BookingDAL();

        private readonly string[] dsSan =
        {
            "SB0001","SB0002","SB0003","SB0004",
            "SB0005","SB0006","SB0007","SB0008"
        };

        public Booking()
        {
            InitializeComponent();
            dpNgayDat.SelectedDate = DateTime.Today;
            LoadSan();
            DisableForm();
        }

        private void LoadSan()
        {
            var all = courtDAL.GetAll();

            foreach (var ma in dsSan)
            {
                var court = all.FirstOrDefault(x => x.MaSan == ma);
                UpdateUI(ma, court?.TrangThai ?? "Trống");
            }
        }

        private void UpdateUI(string maSan, string status)
        {
            var txt = FindName("txtStatus_" + maSan) as TextBlock;
            var btn = FindName("btn_" + maSan) as Button;
            var txtGia = FindName("txtGia_" + maSan) as TextBlock;

            if (txt == null || btn == null) return;

            var court = courtDAL.GetByMaSan(maSan);
            if (court != null && txtGia != null)
                txtGia.Text = $"{court.GiaThue:N0}/h";

            btn.Tag = maSan;

            if (status == "Bảo trì")
            {
                txt.Text = "BẢO TRÌ";
                txt.Foreground = Brushes.Orange;
                btn.Content = "Bảo trì";
                btn.Background = Brushes.Yellow;
                btn.IsEnabled = false;
                return;
            }

            if (!TryGetTime(out TimeSpan bd, out TimeSpan kt))
            {
                SetTrong(txt, btn);
                return;
            }

            bool daDat = bus.KiemTraSanDaDat(
                maSan,
                dpNgayDat.SelectedDate ?? DateTime.Today,
                bd,
                kt
            );

            if (daDat) SetDaDat(txt, btn);
            else SetTrong(txt, btn);
        }

        private void SetDaDat(TextBlock txt, Button btn)
        {
            txt.Text = "ĐÃ ĐẶT";
            txt.Foreground = Brushes.Red;
            btn.Content = "THANH TOÁN";
            btn.Background = Brushes.Red;
        }

        private void SetTrong(TextBlock txt, Button btn)
        {
            txt.Text = "TRỐNG";
            txt.Foreground = Brushes.Green;
            btn.Content = "ĐẶT SÂN";
            btn.Background = Brushes.Green;
        }

        private void ChonSan(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            maSanDangChon = btn.Tag?.ToString() ?? "";

            if (string.IsNullOrEmpty(maSanDangChon)) return;

            if (!TryGetTime(out TimeSpan bd, out TimeSpan kt))
            {
                MessageBox.Show("Chọn giờ!");
                return;
            }

            bool daDat = bus.KiemTraSanDaDat(
                maSanDangChon,
                dpNgayDat.SelectedDate ?? DateTime.Today,
                bd,
                kt
            );

            if (daDat)
                LoadThongTin(bd, kt);
            else
            {
                ResetForm();
                EnableForm();
                btnXacNhan.Content = "XÁC NHẬN ĐẶT SÂN";
                CapNhatTien();
            }
        }

        private void LoadThongTin(TimeSpan bd, TimeSpan kt)
        {
            var dr = bookingDAL.GetThongTinKhachDatSanChuaThanhToan(
                maSanDangChon,
                dpNgayDat.SelectedDate ?? DateTime.Today,
                bd,
                kt
            );

            if (dr == null) return;

            txtTenKH.Text = dr["HoTen"]?.ToString() ?? "";
            txtSDT.Text = dr["SDT"]?.ToString() ?? "";

            TimeSpan gioBD = dr["GioBD"] != DBNull.Value ? (TimeSpan)dr["GioBD"] : TimeSpan.Zero;
            TimeSpan gioKT = dr["GioKT"] != DBNull.Value ? (TimeSpan)dr["GioKT"] : TimeSpan.Zero;

            SetTime(cboGioBD, gioBD);
            SetTime(cboGioKT, gioKT);

            lblTongGio.Text = $"{(gioKT - gioBD).TotalHours} giờ";

            decimal tien = dr["ThanhTien"] != DBNull.Value
                ? Convert.ToDecimal(dr["ThanhTien"])
                : 0;

            lblTamTinh.Text = $"{tien:N0} VNĐ";

            DisableForm();
            btnXacNhan.Content = "THANH TOÁN";
        }

        private void CapNhatTien()
        {
            if (string.IsNullOrEmpty(maSanDangChon)) return;

            if (!TryGetTime(out TimeSpan bd, out TimeSpan kt))
            {
                lblTongGio.Text = "0 giờ";
                lblTamTinh.Text = "0 VNĐ";
                return;
            }

            var court = courtDAL.GetByMaSan(maSanDangChon);
            decimal gia = court != null ? Convert.ToDecimal(court.GiaThue) : 0;

            decimal tien = bus.TinhTienPublic(bd, kt, gia);

            lblTongGio.Text = $"{(kt - bd).TotalHours} giờ";
            lblTamTinh.Text = $"{tien:N0} VNĐ";
        }

        private void DatSan()
        {
            if (!TryGetTime(out TimeSpan bd, out TimeSpan kt)) return;

            var court = courtDAL.GetByMaSan(maSanDangChon);

            var booking = new BookingDTO
            {
                MaSan = maSanDangChon,
                TenKhachHang = txtTenKH.Text.Trim(),
                SDT = txtSDT.Text.Trim(),
                NgayDat = dpNgayDat.SelectedDate ?? DateTime.Today,
                GioBatDau = bd,
                GioKetThuc = kt,
                GiaThue = court != null ? Convert.ToDecimal(court.GiaThue) : 0
            };

            var result = bus.ThucHienDatSan(booking);

            MessageBox.Show(result.Item2);
            if (result.Item1)
            {
                ResetForm();
                LoadSan();
            }
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

        private void btnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            string action = btnXacNhan.Content?.ToString() ?? "";

            if (action == "ĐẶT SÂN")
            {
                DatSan();
                return;
            }

            if (!TryGetTime(out TimeSpan bd, out TimeSpan kt)) return;

            var dr = bookingDAL.GetThongTinKhachDatSanChuaThanhToan(
                maSanDangChon,
                dpNgayDat.SelectedDate ?? DateTime.Today,
                bd,
                kt
            );

            if (dr == null) return;

            var result = bus.ThanhToan(
                dr["MaDatSan"]?.ToString() ?? "",
                dr["SDT"]?.ToString() ?? ""
            );

            MessageBox.Show(result.Item2);
            if (result.Item1)
            {
                ResetForm();
                LoadSan();
            }
        }

        private bool TryGetTime(out TimeSpan bd, out TimeSpan kt)
        {
            bd = TimeSpan.Zero;
            kt = TimeSpan.Zero;

            if (cboGioBD.SelectedItem == null || cboGioKT.SelectedItem == null)
                return false;

            return TimeSpan.TryParse(
                       ((ComboBoxItem)cboGioBD.SelectedItem).Content.ToString(),
                       out bd)
                && TimeSpan.TryParse(
                       ((ComboBoxItem)cboGioKT.SelectedItem).Content.ToString(),
                       out kt)
                && kt > bd;
        }

        private void SetTime(ComboBox cb, TimeSpan t)
        {
            var item = cb.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(x => TimeSpan.Parse(x.Content.ToString()) == t);

            if (item != null) cb.SelectedItem = item;
        }

        private void ResetForm()
        {
            txtTenKH.Clear();
            txtSDT.Clear();
            lblTamTinh.Text = "0 VNĐ";
            lblTongGio.Text = "0 giờ";
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
            LoadSan();
        }

        private void cboGioKT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CapNhatTien();
            LoadSan();
        }

        private void dpNgayDat_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSan();
        }

       
    }
}