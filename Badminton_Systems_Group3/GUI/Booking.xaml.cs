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

        // ================= UI =================
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

        // ================= LỌC SÂN =================
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

        // ================= CLICK CHUNG =================
        private void SB0001_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag == null) return;

            maSanDangChon = btn.Tag.ToString()!;
            DateTime ngay = dpNgayDat.SelectedDate ?? DateTime.Today;

            bool coBooking = bus.KiemTraSanDaDat(maSanDangChon, ngay);

            if (coBooking)
            {
                LoadThongTin(); // load dữ liệu khách chưa thanh toán
            }
            else
            {
                ResetForm();
                EnableForm();
                btnXacNhan.Content = "ĐẶT SÂN";
                CapNhatTien();
            }
        }

        // ================= LOAD THÔNG TIN =================
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

        // ================= TÍNH TIỀN =================
        // Sửa lại hàm CapNhatTien()
        private void CapNhatTien()
        {
            if (string.IsNullOrEmpty(maSanDangChon)) return;

            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT))
            {
                lblTongGio.Text = "0 giờ";
                lblTamTinh.Text = "0 VNĐ";
                return;
            }

            // LẤY GIÁ TỪ QUẢN LÝ SÂN
            CourtDAL courtDAL = new CourtDAL();
            var court = courtDAL.GetByMaSan(maSanDangChon);
            decimal giaThue = court != null ? (decimal)court.GiaThue : 0;

            double gio = (gioKT - gioBD).TotalHours;
            lblTongGio.Text = $"{gio} giờ";

            decimal tien = (decimal)gio * giaThue; // Dùng giá từ DB thay vì 120000
            lblTamTinh.Text = string.Format("{0:N0} VNĐ", tien);
        }

        // Sửa lại hàm DatSan() để gán giá thuê thật trước khi lưu
        private void DatSan()
        {
            // 1. Kiểm tra đầu vào cơ bản
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

            // 2. Lấy thông tin sân từ database để lấy GIÁ THUÊ thực tế
            CourtDAL courtDAL = new CourtDAL();
            var court = courtDAL.GetByMaSan(maSanDangChon);

            if (court == null)
            {
                MessageBox.Show("Không tìm thấy thông tin sân trong hệ thống!");
                return;
            }

            // 3. Khởi tạo đối tượng DTO và gán dữ liệu
            BookingDTO booking = new BookingDTO
            {
                MaSan = maSanDangChon,
                TenKhachHang = txtTenKH.Text.Trim(),
                SDT = txtSDT.Text.Trim(),
                NgayDat = dpNgayDat.SelectedDate.Value,
                GioBatDau = gioBD,
                GioKetThuc = gioKT,
                // Ép kiểu an toàn từ database, nếu null thì mặc định là 0
                GiaThue = court.GiaThue != null ? Convert.ToDecimal(court.GiaThue) : 0
            };

            // 4. Tính toán thành tiền dựa trên số giờ và giá thuê
            booking.TinhThanhTien();

            // 5. Gọi lớp BUS để xử lý nghiệp vụ (Kiểm tra trùng lịch, Lưu khách hàng, Lưu đơn đặt)
            var result = bus.ThucHienDatSan(booking);

            // 6. Xử lý sau khi đặt thành công
            if (result.success)
            {
                // Cập nhật trạng thái sân sang "Đã đặt" để hiển thị màu Đỏ trên giao diện
                court.TrangThai = "Đã đặt";
                courtDAL.Update(court);

                // Làm mới form và tải lại danh sách sân để cập nhật màu sắc UI
                ResetForm();
                LocSan();
            }

            // Hiển thị thông báo cho người dùng (Thành công hoặc lỗi từ BUS)
            MessageBox.Show(result.message);
        }

        // ================= NÚT XÁC NHẬN =================
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

        // ================= FORM =================
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

        // ================= HỖ TRỢ =================
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