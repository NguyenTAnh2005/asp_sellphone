using old_phone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace old_phone.ViewModels
{
    public class OrderItemViewModel
    {
        public int variant_id { get; set; }
        public string name { get; set; }
        public int ram { get; set; }
        public int rom { get; set; }
        public int count { get; set; }
        public decimal price { get; set; }
        public string image_url { get; set; }
    }
    public class OrderViewModel
    {
        public List<Hotline> listHotlines { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }

        // Cac thuoc tinh nhan du lieu tu form post len
        public int Order_HotlineId { get; set; }
        public string Order_TypePayment { get; set; }
    }

    // ViewModel dùng để hiển thị 1 đơn hàng trong danh sách
    public class OrderHistoryViewModel
    {
        public int order_id { get; set; }
        public DateTime order_buy_time { get; set; }
        public string order_state { get; set; } // Trạng thái: PREPARE, SHIPPING, SUCCESS...
        public long order_total_price { get; set; }
        public string order_type_pay { get; set; }

        // Chi tiết 3 mặt hàng đầu tiên để hiển thị nhanh
        public List<string> ItemNames { get; set; }
        public int TotalItemCount { get; set; } // Tổng số lượng mặt hàng (ví dụ: 5 sản phẩm)
    }

    // ViewModel dùng để hiển thị toàn bộ chi tiết 1 đơn hàng cụ thể
    public class OrderDetailViewModel
    {
        // Thông tin đơn hàng chính
        public int OrderId { get; set; }
        public DateTime BuyTime { get; set; }
        public string State { get; set; }
        public long TotalPrice { get; set; }
        public string PaymentType { get; set; }

        // Thông tin địa chỉ nhận hàng
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientAddress { get; set; }

        // Danh sách sản phẩm chi tiết trong đơn hàng
        public List<OrderDetailItem> Items { get; set; }
    }

    // Class chi tiết từng sản phẩm trong đơn hàng
    public class OrderDetailItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public long UnitPrice { get; set; } // Giá lúc mua
        public string ImageUrl { get; set; }
        // Có thể thêm RAM/ROM nếu cần (nhưng ở đây ta chỉ dùng thông tin từ Detail)
    }
}