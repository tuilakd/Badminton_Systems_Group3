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

        private void UpdateUI(string maSan, string status)
        {
            var txt = FindName("txtStatus_" + maSan) as TextBlock;
            var btn = FindName("btn_" + maSan) as Button;

            if (txt == null || btn == null) return;

            btn.IsEnabled = true;

            if (status == "Bảo trì")
            {
                txt.Text = "BẢO TRÌ";
                txt.Foreground = Brushes.Orange; 

                btn.Content = "Bảo trì";
                btn.Background = Brushes.Yellow; 
                btn.Foreground = Brushes.Black;  
                btn.IsEnabled = false;           
            }
            else if (status == "Đã đặt")
            {
                txt.Text = "ĐÃ ĐẶT";
                txt.Foreground = Brushes.Red;

                btn.Content = "THANH TOÁN";
                btn.Background = Brushes.Red;
                btn.Foreground = Brushes.White;
            }
            else 
            {
                txt.Text = "TRỐNG";
                txt.Foreground = Brushes.Green;

                btn.Content = "Đặt sân";
                btn.Background = Brushes.Green;
                btn.Foreground = Brushes.White;
            }
        }

        private void LoadSanMacDinh()
        {
            CourtDAL courtDAL = new CourtDAL();
            var allCourts = courtDAL.GetAll();

            foreach (var ma in dsSan)
            {
                var court = allCourts.FirstOrDefault(c => c.MaSan == ma);
                string status = (court != null && court.TrangThai == "Bảo trì") ? "Bảo trì" : "Trống";
                UpdateUI(ma, status);
            }
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
            var busyList = dt.AsEnumerable().Select(r => r["MaSan"]?.ToString() ?? "").ToList();

            CourtDAL courtDAL = new CourtDAL();
            var allCourts = courtDAL.GetAll();

            foreach (var ma in dsSan)
            {
                var court = allCourts.FirstOrDefault(c => c.MaSan == ma);
                string currentStatus = "Trống";

                if (court != null && court.TrangThai == "Bảo trì")
                {
                    currentStatus = "Bảo trì";
                }
                else if (busyList.Contains(ma))
                {
                    currentStatus = "Đã đặt";
                }

                UpdateUI(ma, currentStatus);
            }
        }

        private void SB0001_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag == null) return;

            maSanDangChon = btn.Tag.ToString()!;
            DateTime ngay = dpNgayDat.SelectedDate ?? DateTime.Today;

            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT)) return;

            bool coBooking = bus.KiemTraSanDaDat(maSanDangChon, ngay, gioBD, gioKT);

            if (coBooking)
            {
                LoadThongTin(gioBD, gioKT);
            }
            else
            {
                ResetForm();
                EnableForm();
                btnXacNhan.Content = "ĐẶT SÂN";
                CapNhatTien();
            }
        }

        private void LoadThongTin(TimeSpan gioBatDau, TimeSpan gioKetThuc)
        {
            if (string.IsNullOrEmpty(maSanDangChon) || dpNgayDat.SelectedDate == null) return;

            DataRow dr = dal.GetThongTinKhachDatSanChuaThanhToan(maSanDangChon, dpNgayDat.SelectedDate.Value, gioBatDau, gioKetThuc);

            if (dr == null)
            {
                MessageBox.Show("Không tìm thấy booking chưa thanh toán!");
                return;
            }

            maDatSanDangChon = dr["MaDatSan"].ToString();
            txtTenKH.Text = dr["HoTen"].ToString();
            txtSDT.Text = dr["SDT"].ToString();

            TimeSpan gioBD_DB = (TimeSpan)dr["GioBD"];
            TimeSpan gioKT_DB = (TimeSpan)dr["GioKT"];

            SetComboBoxTime(cboGioBD, gioBD_DB);
            SetComboBoxTime(cboGioKT, gioKT_DB);

            lblTongGio.Text = $"{(gioKT_DB - gioBD_DB).TotalHours} giờ";
            decimal thanhTien = Convert.ToDecimal(dr["ThanhTien"]);
            lblTamTinh.Text = string.Format("{0:N0} VNĐ", thanhTien);

            btnXacNhan.Content = "THANH TOÁN";
            DisableForm();
        }

        private void CapNhatTien()
        {
            if (string.IsNullOrEmpty(maSanDangChon)) return;

            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT))
            {
                lblTongGio.Text = "0 giờ";
                lblTamTinh.Text = "0 VNĐ";
                return;
            }

            CourtDAL courtDAL = new CourtDAL();
            var court = courtDAL.GetByMaSan(maSanDangChon);

            decimal giaThue = 0;
            if (court != null)
            {
                giaThue = Convert.ToDecimal(court.GiaThue);
            }

            double tongSoGio = (gioKT - gioBD).TotalHours;

            if (tongSoGio < 0) tongSoGio = 0;

            decimal tongTien = (decimal)tongSoGio * giaThue;

            lblTongGio.Text = $"{tongSoGio} giờ";
            lblTamTinh.Text = string.Format("{0:N0} VNĐ", tongTien);
        }

        private void DatSan()
        {
            if (dpNgayDat.SelectedDate == null || string.IsNullOrEmpty(maSanDangChon))
            {
                MessageBox.Show("Vui lòng chọn ngày và sân trước khi đặt!");
                return;
            }

            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT))
            {
                MessageBox.Show("Khung giờ chọn không hợp lệ!");
                return;
            }

            CourtDAL courtDAL = new CourtDAL();
            var court = courtDAL.GetByMaSan(maSanDangChon);

            if (court == null)
            {
                MessageBox.Show("Không tìm thấy thông tin sân trong hệ thống!");
                return;
            }

            BookingDTO booking = new BookingDTO
            {
                MaSan = maSanDangChon,
                TenKhachHang = txtTenKH.Text.Trim(),
                SDT = txtSDT.Text.Trim(),
                NgayDat = dpNgayDat.SelectedDate.Value,
                GioBatDau = gioBD,
                GioKetThuc = gioKT,
                GiaThue = court.GiaThue != null ? Convert.ToDecimal(court.GiaThue) : 0
            };

            booking.TinhThanhTien();

            var result = bus.ThucHienDatSan(booking);

            if (result.success)
            {
                court.TrangThai = "Đã đặt";
                courtDAL.Update(court);

                ResetForm();
                LocSan();
            }

            MessageBox.Show(result.message);
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
                if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT)) return;

                var drTemp = dal.GetThongTinKhachDatSanChuaThanhToan(maSanDangChon, dpNgayDat.SelectedDate ?? DateTime.Today, gioBD, gioKT);

                if (drTemp == null) return;

                string maDS = drTemp["MaDatSan"]?.ToString() ?? "";
                string sdtKhach = drTemp["SDT"]?.ToString() ?? "";
                decimal tien = Convert.ToDecimal(drTemp["ThanhTien"]);

                var result = bus.ThanhToan(maDS, sdtKhach, tien);

                MessageBox.Show(result.message);
                if (result.success)
                {
                    CourtDAL courtDAL = new CourtDAL();
                    var court = courtDAL.GetByMaSan(maSanDangChon);
                    if (court != null)
                    {
                        court.TrangThai = "Trống"; 
                        courtDAL.Update(court);    
                    }

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