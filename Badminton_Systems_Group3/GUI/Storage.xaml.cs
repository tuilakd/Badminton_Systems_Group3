using System;
using System.Collections.Generic;
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
    /// Interaction logic for Storage.xaml
    /// </summary>
    public partial class Storage : Window
    {
<<<<<<< Updated upstream
=======
        BUS.StorageBUS bus = new BUS.StorageBUS();

>>>>>>> Stashed changes
        public Storage()
        {
            InitializeComponent();
        }

       

        private void btnThongTin_Click(object sender, RoutedEventArgs e)
        {
            Badminton_Systems_Group3.GUI.Info thongTinWindow = new Badminton_Systems_Group3.GUI.Info();
            thongTinWindow.ShowDialog();

            btnThongTin.IsChecked = false;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
<<<<<<< Updated upstream
            Home window = new Home();
            window.Show();
            this.Close();
=======
            var row = dgSanPham.SelectedItem as DataRowView;
            if (row == null) return;

            txtMaSP.Text = row["MaSP"].ToString();
            txtTenSP.Text = row["TenSP"].ToString();
            txtDonGia.Text = row["DonGia"].ToString();
            txtSoLuong.Text = row["TonKho"].ToString();
            dpNgayNhap.SelectedDate = Convert.ToDateTime(row["NgayNhap"]);
        }

        // ================= LỌC CHECKBOX =================
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

            if (chkQuanCanHBT.IsChecked == true)
                chon.Add("Quấn cán HBT");

            if (chkCauThanhCong.IsChecked == true)
                chon.Add("Cầu Thành công");

            if (chkCauXSmash.IsChecked == true)
                chon.Add("Cầu XSmash");

            if (chon.Count > 0)
            {
                var filtered = dt.AsEnumerable()
                                 .Where(x => chon.Contains(x.Field<string>("TenSP")))
                                 .AsDataView();

                dgSanPham.ItemsSource = filtered;
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

        // ================= NHẬP KHO =================
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaSP.Text) ||
                string.IsNullOrEmpty(txtTenSP.Text) ||
                string.IsNullOrEmpty(txtDonGia.Text) ||
                string.IsNullOrEmpty(txtSoLuong.Text))
            {
                MessageBox.Show("Nhập thiếu!");
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

            StorageDTO sp = new StorageDTO()
            {
                MaSP = txtMaSP.Text,
                TenSP = txtTenSP.Text,
                DonGia = gia,
                SoLuongTon = sl,
                NgayNhap = dpNgayNhap.SelectedDate ?? DateTime.Now
            };

            string result = bus.ThucHienNhapKho(sp);

            MessageBox.Show(result);
            LoadData();
            ClearForm();
        }

        // ================= KIỂM KÊ =================
        private void btnKiemKe_Click(object sender, RoutedEventArgs e)
        {
            var row = dgSanPham.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Chọn sản phẩm!");
                return;
            }

            int slThucTe;
            if (!int.TryParse(txtSoLuong.Text, out slThucTe))
            {
                MessageBox.Show("Nhập số lượng hợp lệ!");
                return;
            }

            int soLuongTon = Convert.ToInt32(row["TonKho"]);
            int chenhLech = slThucTe - soLuongTon;

            if (chenhLech == 0)
            {
                MessageBox.Show("Kho chính xác!");
                return;
            }

            string tinhTrang = chenhLech > 0 ? "thừa" : "thiếu";

            if (MessageBox.Show($"Kho đang {tinhTrang} {Math.Abs(chenhLech)}. Cập nhật?",
                "Kiểm kê", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                StorageDTO sp = new StorageDTO()
                {
                    MaSP = row["MaSP"].ToString(),
                    TenSP = row["TenSP"].ToString(),
                    DonGia = Convert.ToDouble(row["DonGia"]),
                    SoLuongTon = chenhLech,
                    NgayNhap = DateTime.Now
                };

                string kq = bus.ThucHienNhapKho(sp);

                MessageBox.Show(kq);
                LoadData();
                ClearForm();
            }
        }

        // ================= CLEAR =================
        void ClearForm()
        {
            txtMaSP.Clear();
            txtTenSP.Clear();
            txtDonGia.Clear();
            txtSoLuong.Clear();
            dpNgayNhap.SelectedDate = DateTime.Now;
        }

        private void txtDonGia_TextChanged(object sender, TextChangedEventArgs e)
        {

>>>>>>> Stashed changes
        }
    }
}
