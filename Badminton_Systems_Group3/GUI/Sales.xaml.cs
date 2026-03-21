using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace Badminton_Systems_Group3.GUI

{
    public partial class Sales : Window
    {
        private SalesBUS bus = new SalesBUS();
        public ObservableCollection<SalesDTO> cart { get; set; } = new ObservableCollection<SalesDTO>();

        public Sales()
        {
            InitializeComponent();
            LoadProducts();
            dgHoaDon.ItemsSource = cart;
        }
        private void LoadProducts()
        {
            var allProducts = bus.GetAllProducts();
            icProducts.ItemsSource = allProducts; 
            if (allProducts != null)
    {
        itemSP0001.DataContext = allProducts.FirstOrDefault(x => x.MaSP == "SP0001");
        itemSP0002.DataContext = allProducts.FirstOrDefault(x => x.MaSP == "SP0002");
        itemSP0003.DataContext = allProducts.FirstOrDefault(x => x.MaSP == "SP0003");
        itemSP0004.DataContext = allProducts.FirstOrDefault(x => x.MaSP == "SP0004");
        itemSP0005.DataContext = allProducts.FirstOrDefault(x => x.MaSP == "SP0005");
        itemSP0006.DataContext = allProducts.FirstOrDefault(x => x.MaSP == "SP0006");
    }
        }
        private void btnThem_Static_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string maSP = btn.Tag?.ToString();

            var allProducts = bus.GetAllProducts();
            var sp = allProducts.FirstOrDefault(x => x.MaSP == maSP);

            if (sp != null)
            {
                AddToCart(sp);
            }
            else
            {
                MessageBox.Show($"Không tìm thấy sản phẩm có mã: '{maSP}' trong Database!");
            }
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var sp = btn.Tag as ProductDTO;
            if (sp != null) { AddToCart(sp); }
        }

        private void AddToCart(ProductDTO sp)
        {
            string error = bus.CheckInventory(sp.MaSP, 1);
            if (error != null) { MessageBox.Show(error); return; }

            var existing = cart.FirstOrDefault(x => x.MaSP == sp.MaSP);
            if (existing != null)
            {
                existing.SoLuong++;
                dgHoaDon.Items.Refresh();
            }
            else
            {
                cart.Add(new SalesDTO { MaSP = sp.MaSP, TenSP = sp.TenSP, DonGia = sp.DonGia, SoLuong = 1 });
            }
            UpdateTotal();
        }

        private void btnTang_Click(object sender, RoutedEventArgs e)
        {
            if (dgHoaDon.SelectedItem is SalesDTO selected)
            {
                if (bus.CheckInventory(selected.MaSP, selected.SoLuong + 1) == null)
                {
                    selected.SoLuong++;
                    txtSoluong.Text = selected.SoLuong.ToString();
                    dgHoaDon.Items.Refresh();
                    UpdateTotal();
                }
                else MessageBox.Show("Hết hàng!");
            }
        }        
        private void btnGiam_Click(object sender, RoutedEventArgs e)
        {
            if (dgHoaDon.SelectedItem is SalesDTO selected && selected.SoLuong > 1)
            {
                selected.SoLuong--;
                txtSoluong.Text = selected.SoLuong.ToString();
                dgHoaDon.Items.Refresh();
                UpdateTotal();
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (dgHoaDon.SelectedItem is SalesDTO selected)
            {
                cart.Remove(selected);
                UpdateTotal();
            }
        }

        private void btnThanhToan_Click(object sender, RoutedEventArgs e)

        {
            if (cart.Count == 0) return;

            string maHD = "HD" + DateTime.Now.ToString("mmss");
            decimal tongTien = cart.Sum(x => x.ThanhTien);

            bool isFatherSaved = bus.CreateHoaDon(maHD, tongTien);

            if (isFatherSaved)
            {
                foreach (var item in cart)
                {
                    bus.SaveBillDetail(maHD, item);
                    bus.UpdateInventory(item.MaSP, item.SoLuong);
                }
                MessageBox.Show("Thanh toán & lưu hóa đơn thành công!");
                cart.Clear();
                UpdateTotal();
                LoadProducts();
            }
            else
            {
                MessageBox.Show("Lỗi: Không thể tạo hóa đơn tổng. Vui lòng kiểm tra tên cột trong bảng 'hoadon'!");
            }
        }

        private void UpdateTotal()
        {
            decimal total = cart.Sum(x => x.ThanhTien);
            lblTong.Text = string.Format("{0:N0} VNĐ", total);
        }

        public class SanPham : INotifyPropertyChanged
        {
            public string MaSP { get; set; }
            public string TenSP { get; set; }
            public decimal DonGia { get; set; }

            private int _soLuongTon;
            public int SoLuongTon
            {
                get => _soLuongTon;
                set
                {
                    _soLuongTon = value;
                    OnPropertyChanged("SoLuongTon");
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private void dgHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgHoaDon.SelectedItem is SalesDTO selected)
            {
                txtSoluong.Text = selected.SoLuong.ToString();
            }
        }

        private void btnThongTin_Click(object sender, RoutedEventArgs e) { new Info().ShowDialog(); btnThongTin.IsChecked = false; }
        private void RadioButton_Checked(object sender, RoutedEventArgs e) { new BookingSchedule().Show(); this.Close(); }
        private void RadioButton_Checked_1(object sender, RoutedEventArgs e) { new Home().Show(); this.Close(); }
    }

}