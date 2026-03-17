

-- 1. Bảng Khách hàng 
CREATE TABLE khachhang (
    MaKH VARCHAR(10) PRIMARY KEY CHECK (MaKH LIKE 'KH[0-9][0-9][0-9][0-9]'),
    HoTen NVARCHAR(100), 
    SDT VARCHAR(20) 
);

-- 2. Bảng Thông tin đăng nhập
CREATE TABLE [user] (
    username VARCHAR(20) PRIMARY KEY, 
    password VARCHAR(100) 
);

-- 3. Bảng Sân bãi 
CREATE TABLE san (
    MaSan VARCHAR(10) PRIMARY KEY CHECK (MaSan LIKE 'SB[0-9][0-9][0-9][0-9]'),
    TenSan NVARCHAR(50),  
    TrangThai NVARCHAR(50),
    GiaThue DECIMAL(10,2) 
);

-- 4. Bảng Sản phẩm
CREATE TABLE sanpham (
    MaSP VARCHAR(10) PRIMARY KEY CHECK (MaSP LIKE 'SP[0-9][0-9][0-9][0-9]'),
    TenSP NVARCHAR(100), 
    DonGia DECIMAL(10,2), 
    SoLuongTon INT 
);

-- 5. Bảng Nhập kho
CREATE TABLE nhapkho (
    MaSP VARCHAR(10), 
    DonGia DECIMAL(10,2), 
    SoLuongNhap INT,
    NgayNhap DATE, 
    PRIMARY KEY (MaSP, NgayNhap), 
    FOREIGN KEY (MaSP) REFERENCES sanpham(MaSP)
);

-- 6. Bảng Đặt sân 
CREATE TABLE datsan (
    MaDatSan VARCHAR(10) PRIMARY KEY CHECK (MaDatSan LIKE 'DS[0-9][0-9][0-9][0-9]'),
    NgayDat DATE, 
    GioBD TIME, 
    GioKT TIME, 
    TrangThai NVARCHAR(100),
    MaKH VARCHAR(10), -- [cite: 31]
    MaSan VARCHAR(10), -- [cite: 31]
    GiaThue DECIMAL(10,2), -- [cite: 31]
    ThanhTien DECIMAL(10,2), -- [cite: 31]
    FOREIGN KEY (MaKH) REFERENCES khachhang(MaKH),
    FOREIGN KEY (MaSan) REFERENCES san(MaSan)
);

-- 7. Bảng Hóa đơn 
CREATE TABLE hoadon (
    MaHD VARCHAR(10) PRIMARY KEY CHECK (MaHD LIKE 'HD[0-9][0-9][0-9][0-9]'),
    MaKH VARCHAR(10), 
    NgayLapHD DATETIME, 
    TongTien DECIMAL(15,2), 
    FOREIGN KEY (MaKH) REFERENCES khachhang(MaKH)
);

-- 8. Bảng Chi tiết hóa đơn đặt sân 
CREATE TABLE chitiethoadon_san (
    MaHD VARCHAR(10), 
    MaDatSan VARCHAR(10), 
    GiaThue DECIMAL(10,2), 
    ThanhTien DECIMAL(10,2), 
    PRIMARY KEY (MaHD, MaDatSan), 
    FOREIGN KEY (MaHD) REFERENCES hoadon(MaHD),
    FOREIGN KEY (MaDatSan) REFERENCES datsan(MaDatSan)
);

-- 9. Bảng Chi tiết hóa đơn sản phẩm [cite: 40]
CREATE TABLE chitiethoadon_sp (
    MaHD VARCHAR(10), 
    MaSP VARCHAR(10), 
    SoLuong INT, 
    DonGia DECIMAL(10,2),
    ThanhTien DECIMAL(10,2), 
    PRIMARY KEY (MaHD, MaSP), 
    FOREIGN KEY (MaHD) REFERENCES hoadon(MaHD),
    FOREIGN KEY (MaSP) REFERENCES sanpham(MaSP)
);

-- 10. Bảng Báo cáo Doanh thu [cite: 45]
CREATE TABLE doanhthu (
    MaBaoCao VARCHAR(10) PRIMARY KEY CHECK (MaBaoCao LIKE 'BC[0-9][0-9][0-9][0-9]'),
    Thang INT, 
    Ngay INT, 
    Nam INT, 
    tongDoanhThu DECIMAL(15,0)
);