
DBCC CHECKIDENT ('Company', RESEED, 0);


SELECT * FROM ACCOUNT 
select * from Blog
SELECT * FROM COMPANY 
SELECT * FROM Detail
select * from Hotline
SELECT * FROM ORDER_
SELECT * FROM Phone
SELECT * FROM Product
SELECT * FROM Product_Image
SELECT * FROM SALE 
SELECT * FROM STOCK
SELECT * FROM Variant_Phone


USE OldPhone;
GO

-- BƯỚC 1: XÓA DỮ LIỆU CÁC BẢNG CON (Cấp thấp nhất)
-- Xóa chi tiết đơn hàng
DELETE FROM Detail;
DBCC CHECKIDENT ('Detail', RESEED, 0); -- Reset ID về 0

-- Xóa giỏ hàng
DELETE FROM Cart;
DBCC CHECKIDENT ('Cart', RESEED, 0);

-- Xóa các đợt Sale
DELETE FROM Sale;
DBCC CHECKIDENT ('Sale', RESEED, 0);

-- Xóa tồn kho (Bảng này PK là variant_id, không có Identity nên không cần Reseed)
DELETE FROM Stock;

-- Xóa thông số kỹ thuật (PK là product_id, không Identity)
DELETE FROM Phone;

-- Xóa ảnh sản phẩm
DELETE FROM Product_Image;
DBCC CHECKIDENT ('Product_Image', RESEED, 0);

-- Xóa Blog
DELETE FROM Blog;
DBCC CHECKIDENT ('Blog', RESEED, 0);

-- BƯỚC 2: XÓA CÁC BẢNG TRUNG GIAN
-- Xóa đơn hàng (Phải xóa Detail trước mới xóa được cái này)
DELETE FROM Order_;
DBCC CHECKIDENT ('Order_', RESEED, 0);

-- Xóa biến thể điện thoại (Phải xóa Cart, Sale, Stock, Detail trước)
DELETE FROM Variant_Phone;
DBCC CHECKIDENT ('Variant_Phone', RESEED, 0);

-- Xóa sổ địa chỉ (Phải xóa Order_ trước)
DELETE FROM Hotline;
DBCC CHECKIDENT ('Hotline', RESEED, 0);

-- BƯỚC 3: XÓA CÁC BẢNG CHA (Cấp cao nhất)
-- Xóa sản phẩm gốc
DELETE FROM Product;
DBCC CHECKIDENT ('Product', RESEED, 0);

-- Xóa hãng sản xuất
DELETE FROM Company;
DBCC CHECKIDENT ('Company', RESEED, 0);

-- Xóa tài khoản
DELETE FROM Account;
DBCC CHECKIDENT ('Account', RESEED, 0);

-- Xóa quyền (Roles)
DELETE FROM Roles;
DBCC CHECKIDENT ('Roles', RESEED, 0);

PRINT '--- Đã xóa sạch dữ liệu và Reset ID thành công! ---'
GO






