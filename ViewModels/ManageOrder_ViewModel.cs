using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace old_phone.ViewModels
{
    // =======================================================
    // 1. Dùng cho Index (Danh sách đơn hàng)
    // =======================================================
    public class AdminOrderHeaderViewModel
    {
        public int OrderId { get; set; }            // Mã hóa đơn
        public DateTime OrderBuyTime { get; set; }  // Ngày đặt
        public long OrderTotalPrice { get; set; }   // Tổng tiền
        public int OrderState { get; set; }         // Trạng thái hóa đơn (số)
        public string OrderStateDisplay { get; set; }// Trạng thái hóa đơn (chữ)
        public string CustomerFullName { get; set; } // Tên khách
        public string RecipientPhoneNumber { get; set; } // SĐT
    }

    // =======================================================
    // 2. Dùng cho Details (Thông tin chi tiết sản phẩm)
    // =======================================================
    public class OrderDetailItemViewModel
    {
        public string MainProductName { get; set; } // Tên sản phẩm gốc (Product.product_name)
        public string ProductName { get; set; } // Tên biến thể (Detail.detail_name)
        public int Quantity { get; set; }
        public long ItemTotalPrice { get; set; }

        // Thông tin cấu hình chi tiết (từ Variant_Phone)
        public int Ram { get; set; }
        public int Rom { get; set; }
        public string Color { get; set; }
    }

    // =======================================================
    // 3. Dùng cho Details và Update Status đơn hàng 
    // =======================================================
    public class StatusOption
    {
        public int Value { get; set; } // Giá trị số (0, 1, 2,...)
        public string Text { get; set; } // Giá trị hiển thị ("Đang xác nhận", ...)
        public bool IsSelected { get; set; } // Dùng để chọn trạng thái hiện tại trong Dropdown
    }

    // =======================================================
    // 4. ViewModel tổng hợp. Dùng cho trang Quản lý đơn hàng (Details + Cập nhật trạng thái)
    // =======================================================
    public class ManageOrder_ViewModel
    {
        // Thông tin Header (tổng quan)
        public AdminOrderHeaderViewModel Header { get; set; }

        // Thông tin bổ sung từ Hotline/Order_
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }

        // Danh sách sản phẩm đã mua
        public List<OrderDetailItemViewModel> Items { get; set; }

        // Danh sách tùy chọn trạng thái (chức năng cập nhật)
        public List<StatusOption> StatusList { get; set; }
        public int SelectedStatus { get; set; }
    }
}