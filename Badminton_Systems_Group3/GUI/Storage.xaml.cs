using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DAL;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    /// <summary>
    //l
    public partial class Storage : Window
    {
        SanPhamBUS bus = new SanPhamBUS();
        public Storage()
        {
            InitializeComponent();
            LoadData();
        }
        void LoadData()
        {
            dgSanPham.ItemsSource = bus.GetAll().DefaultView;
        }
       private void dgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSanPham.SelectedItem == null) return;

            var sp = dgSanPham.SelectedItem as ProductDTO;
            if (sp != null)
            {
                txtMaSP.Text  = sp.MaSP;
                txtTenSP.Text = sp.TenSP;
                txtDonGia.Text = sp.DonGia.ToString();
                txtSoLuong.Text = sp.SoLuongTon.ToString();
                dpNgayNhap.SelectedDate = sp.NgayNhap;
            }
        }
        private void LocSanPham()
        {
            DataTable dt = bus.GetAll();

            List<string> chon = new List<string>();

            if (chkReviveVang.IsChecked == true)
                chon.Add("Revive vàng");

            if (chkReviveTrang.IsChecked == true)
                chon.Add("Revive trắng");

            if (chkNuocLavie.IsChecked == true)
                chon.Add("Nước Lavie 500ml");
            if (chkQuanCanHBT.IsChecked == true) chon.Add("Quấn cán HBT");
            if (chkCauThanhCong.IsChecked == true) chon.Add("Cầu Thành công");
            if (chkCauXSmash.IsChecked == true) chon.Add("Cầu XSmash");

            if (chon.Count > 0)
            {
                var filteredData = dt.AsEnumerable()
                             .Where(x => chon.Contains(x.Field<string>("TenSP")))
                             .AsDataView();
                dgSanPham.ItemsSource = filteredData;
            }
            else
            {
                dgSanPham.ItemsSource = dt.DefaultView;
            }
        }
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            LocSanPham();
        }

        private void btnKiemKe_Click(object sender, RoutedEventArgs e)
        {
            if (dgSanPham.SelectedItem == null)
            {
                MessageBox.Show("Chọn sản phẩm cần kiểm kê từ bảng!");
                return;
            }

            var sp = dgSanPham.SelectedItem as ProductDTO;

            int slThucTe;

            if (int.TryParse(txtSoLuong.Text, out slThucTe))
            {
                MessageBox.Show("Nhập số lượng thực tế!");
                return;
            }

            int chenhLech = slThucTe - sp.SoLuongTon;

            if (chenhLech == 0)
            {
                MessageBox.Show("Số liệu kho trùng khớp thực tế!");
            }
            else
            {
                string tinhTrang = chenhLech > 0 ? "thừa" : "thiếu";
                var result = MessageBox.Show(
            $"Kho đang {tinhTrang} {Math.Abs(chenhLech)} sản phẩm so với thực tế.\n" +
            "Bạn có muốn điều chỉnh lại số liệu kho không?",
            "Xác nhận kiểm kê",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // TẠO ĐỐI TƯỢNG MỚI ĐỂ CẬP NHẬT
                    // Vì hàm ThucHienNhapKho của bạn sẽ cộng dồn (Update)
                    // Ta gửi số lượng chênh lệch (chenhLech) để DAL cộng vào database
                    ProductDTO spDieuChinh = new ProductDTO()
                    {
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        DonGia = sp.DonGia,
                        SoLuongTon = chenhLech, // Gửi phần chênh lệch để cộng dồn
                        NgayNhap = DateTime.Now
                    };

                    // GỌI ĐÚNG HÀM TRONG BUS: ThucHienNhapKho
                    string message = bus.ThucHienNhapKho(spDieuChinh);

                    MessageBox.Show(message);
                    LoadData();  // Cập nhật lại DataGrid
                    ClearForm(); // Xóa sạch form
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSP.Text) || string.IsNullOrEmpty(txtSoLuong.Text) || string.IsNullOrEmpty(txtDonGia.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                return;
            }

        
        double gia;
        int sl;
         if (!double.TryParse(txtDonGia.Text, out gia) ||
            !int.TryParse(txtSoLuong.Text, out sl))
        {
            MessageBox.Show("Sai định dạng!");
            return;
            }
        

        ProductDTO sp = new ProductDTO()
        {
            MaSP = txtMaSP.Text,
            TenSP = txtTenSP.Text,
            DonGia = gia,
            SoLuongTon = sl,
            NgayNhap = dpNgayNhap.SelectedDate ?? DateTime.Now
        };
        string result = bus.ThucHienNhapKho(sp);
 MessageBox.Show(result); // Hiện thông báo theo đặc tả
            LoadData();              // Làm mới bảng dữ liệu
    ClearForm();             // Xóa form sau khi nhập
}

void ClearForm()
{
            txtMaSP.Clear();
            txtTenSP.Clear();
            txtDonGia.Clear();
            txtSoLuong.Clear();
            dpNgayNhap.SelectedDate = DateTime.Now;
        }
}
    }

