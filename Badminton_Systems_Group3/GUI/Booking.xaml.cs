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
        private void UpdateUI(string maSan, string statusVatLy)
        {
            var txtStatus = FindName("txtStatus_" + maSan) as TextBlock;
            var btn = FindName("btn_" + maSan) as Button;
            var txtGia = FindName("txtGia_" + maSan) as TextBlock;

            if (txtStatus == null || btn == null) return;

            // 1. CẬP NHẬT GIÁ THUÊ TỪ DATABASE (Để hiện 130k, 150k... thay vì 120k cố định)
            CourtDAL courtDAL = new CourtDAL();
            var court = courtDAL.GetByMaSan(maSan);
            if (court != null && txtGia != null)
            {
                txtGia.Text = string.Format("{0:N0}/h", court.GiaThue);
            }

            btn.Tag = maSan;
            btn.IsEnabled = true;

            // 2. PHÂN LOẠI TRẠNG THÁI HIỂN THỊ
            if (statusVatLy == "Bảo trì")
            {
                txtStatus.Text = "BẢO TRÌ";
                txtStatus.Foreground = Brushes.Orange;
                btn.Content = "Bảo trì";
                btn.Background = Brushes.Yellow;
                btn.Foreground = Brushes.Black;
                btn.IsEnabled = false; // Khóa sân bảo trì
            }
            else
            {
                // Nếu sân Đang hoạt động, kiểm tra xem ngày hiện tại đã có ai đặt chưa
                DateTime ngayChon = dpNgayDat.SelectedDate ?? DateTime.Today;
                bool daCoLich = bus.KiemTraSanDaDat(maSan, ngayChon);

                if (daCoLich)
                {
                    txtStatus.Text = "ĐÃ ĐẶT";
                    txtStatus.Foreground = Brushes.Red;
                    btn.Content = "THANH TOÁN";
                    btn.Background = Brushes.Red;
                    btn.Foreground = Brushes.White;
                }
                else
                {
                    txtStatus.Text = "TRỐNG";
                    txtStatus.Foreground = Brushes.Green;
                    btn.Content = "Đặt sân";
                    btn.Background = Brushes.Green;
                    btn.Foreground = Brushes.White;
                }
            }
        }

        private void LoadSanMacDinh()
        {
            CourtDAL courtDAL = new CourtDAL();
            var allCourts = courtDAL.GetAll(); // Lấy toàn bộ danh sách sân từ DB

            foreach (var ma in dsSan)
            {
                var court = allCourts.FirstOrDefault(c => c.MaSan == ma);
                if (court != null)
                {
                    // Truyền trực tiếp Trạng thái từ DB (Đang hoạt động / Bảo trì)
                    // Logic hiển thị màu sắc sẽ do UpdateUI quyết định dựa trên lịch đặt
                    UpdateUI(ma, court.TrangThai);
                }
            }
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

            // 1. Lấy danh sách các sân đã có người đặt trong khung giờ này
            DataTable dt = bus.GetSanDaDat(dpNgayDat.SelectedDate.Value, gioBD, gioKT);
            var busyList = dt.AsEnumerable().Select(r => r["MaSan"]?.ToString() ?? "").ToList();

            // 2. Lấy toàn bộ thông tin sân để check trạng thái "Bảo trì"
            CourtDAL courtDAL = new CourtDAL();
            var allCourts = courtDAL.GetAll(); // Giả sử bạn có hàm GetAll trả về List<CourtDTO>

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

        // ================= TÍNH TIỀN =================
        private void CapNhatTien()
        {
            // 1. Kiểm tra mã sân có đang được chọn hay không
            if (string.IsNullOrEmpty(maSanDangChon)) return;

            // 2. Lấy thời gian từ ComboBox, nếu không hợp lệ thì reset nhãn hiển thị
            if (!TryGetTimeFromComboBox(out TimeSpan gioBD, out TimeSpan gioKT))
            {
                lblTongGio.Text = "0 giờ";
                lblTamTinh.Text = "0 VNĐ";
                return;
            }

            // 3. Lấy thông tin sân từ Database để lấy giá thuê thực tế
            CourtDAL courtDAL = new CourtDAL();
            var court = courtDAL.GetByMaSan(maSanDangChon);

            // 4. Xử lý ép kiểu an toàn: Chuyển từ double sang decimal để tính tiền chính xác
            // Sử dụng Convert.ToDecimal để tránh lỗi InvalidCastException nếu dữ liệu không khớp
            decimal giaThue = 0;
            if (court != null)
            {
                giaThue = Convert.ToDecimal(court.GiaThue);
            }

            // 5. Tính toán số giờ và tổng tiền
            double tongSoGio = (gioKT - gioBD).TotalHours;

            // Đảm bảo số giờ không âm (phòng trường hợp logic TryGetTime bị sót)
            if (tongSoGio < 0) tongSoGio = 0;

            decimal tongTien = (decimal)tongSoGio * giaThue;

            // 6. Cập nhật giao diện (UI)
            lblTongGio.Text = $"{tongSoGio} giờ";
            lblTamTinh.Text = string.Format("{0:N0} VNĐ", tongTien);
        }

        // ================= ĐẶT SÂN =================
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
                if (drTemp == null) return;

                string maDS = drTemp["MaDatSan"]?.ToString() ?? "";
                string sdtKhach = drTemp["SDT"]?.ToString() ?? "";
                decimal tien = Convert.ToDecimal(drTemp["ThanhTien"]);

                // Gọi hàm BUS (đảm bảo hàm này trong BUS nhận: string, string, decimal)
                var result = bus.ThanhToan(maDS, sdtKhach, tien);

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