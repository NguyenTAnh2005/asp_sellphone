using old_phone.Models;
using System;
using System.Collections.Generic;

namespace old_phone.ViewModels
{
    // ViewModel chính cho Dashboard
    public class DashboardViewModel
    {
        // === 4 CARDS TH?NG KÊ T?NG QUAN ===
        public int TotalProducts { get; set; }           // T?ng s? variant ?ang bán
        public decimal TotalRevenue { get; set; }        // T?ng doanh thu (order_state = 3)
        public int TotalOrders { get; set; }             // T?ng s? ??n hàng
        public int TotalMembers { get; set; }            // T?ng s? User

        // === 2 C?NH BÁO ===
        public List<ExpiringSaleAlert> ExpiringSales { get; set; }     // Sale s?p h?t h?n (3 ngày)
        public List<LowStockAlert> LowStockProducts { get; set; }      // S?n ph?m t?n kho < 5

        // === BI?U ?? DOANH THU 7 NGÀY ===
        public List<RevenueByDay> RevenueChart { get; set; }
    }

    // Class cho Sale s?p h?t h?n
    public class ExpiringSaleAlert
    {
        public int SaleId { get; set; }
        public string SaleName { get; set; }
        public DateTime SaleEnd { get; set; }
        public string ProductName { get; set; }
        public string VariantInfo { get; set; }         // VD: "?en 8/128GB"
        public int DaysRemaining { get; set; }          // S? ngày còn l?i
    }

    // Class cho s?n ph?m s?p h?t hàng
    public class LowStockAlert
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantColor { get; set; }
        public int Ram { get; set; }
        public int Rom { get; set; }
        public int StockCount { get; set; }             // S? l??ng còn l?i
        public string ProductImage { get; set; }
    }

    // Class cho bi?u ?? doanh thu theo ngày
    public class RevenueByDay
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }
}
