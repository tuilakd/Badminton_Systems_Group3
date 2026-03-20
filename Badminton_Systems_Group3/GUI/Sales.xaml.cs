using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Badminton_Systems_Group3.BUS;
using Badminton_Systems_Group3.DTO;

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
            icProducts.ItemsSource = allProducts; // Hiện thẻ sản phẩm
        }

        // Xử lý nút gõ tay
        private void btnThem_Static_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string maSP = btn.Tag?.ToString();

            // Lấy list sản phẩm đang có
            var allProducts = bus.GetAllProducts();
            var sp = allProducts.FirstOrDefault(x => x.MaSP == maSP);

            if (sp != null)
            {
                AddToCart(sp);
            }
            else
            {
                // Nếu nó chui vào đây là do cái Tag "SP000x" của bà không giống trong DB
                MessageBox.Show($"Không tìm thấy sản phẩm có mã: '{maSP}' trong Database!");
            }
        }

        // Xử lý nút động từ SQL
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
            if (cart.Count == 0) { MessageBox.Show("Hóa đơn đang trống!"); return; }

            // 1. CHẠY LỆNH TRỪ KHO TRONG DATABASE
            foreach (var item in cart)
            {
                // Gọi xuống BUS để thực hiện lệnh UPDATE SQL
                bus.UpdateInventory(item.MaSP, item.SoLuong);
            }

            MessageBox.Show("Thanh toán thành công và đã trừ tồn kho!");

            // 2. CẬP NHẬT LẠI GIAO DIỆN
            cart.Clear();          // Xóa giỏ hàng
            UpdateTotal();         // Cập nhật lại tổng tiền về 0
            LoadProducts();        // QUAN TRỌNG: Load lại sản phẩm từ DB để cập nhật con số "Kho" mới
        }

        private void UpdateTotal()
        {
            decimal total = cart.Sum(x => x.ThanhTien);
            lblTong.Text = string.Format("{0:N0} VNĐ", total);
        }

        private void dgHoaDon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgHoaDon.SelectedItem is SalesDTO selected)
            {
                txtSoluong.Text = selected.SoLuong.ToString();
            }
        }
       

        // Các hàm điều hướng
        private void btnThongTin_Click(object sender, RoutedEventArgs e) { new Info().ShowDialog(); btnThongTin.IsChecked = false; }
        private void RadioButton_Checked(object sender, RoutedEventArgs e) { new BookingSchedule().Show(); this.Close(); }
        private void RadioButton_Checked_1(object sender, RoutedEventArgs e) { new Home().Show(); this.Close(); }
    }


}