using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using old_phone.Common;
using old_phone.Models;
using old_phone.ViewModels;

namespace old_phone.Controllers.Manage
{
    public class DashboardController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: Dashboard
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Index()
        {
            var model = new DashboardViewModel();

            // === 1. T?NG S? S?N PH?M ?ANG BÁN ===
            model.TotalProducts = db.Variant_Phone.Count();

            // === 2. T?NG DOANH THU (CH? ??N HÀNG ?Ã GIAO - STATUS = 3) ===
            model.TotalRevenue = db.Order_
                .Where(o => o.order_state == 3)
                .Sum(o => (decimal?)o.order_total_price) ?? 0;

            // === 3. T?NG S? ??N HÀNG ===
            model.TotalOrders = db.Order_.Count();

            // === 4. T?NG S? THÀNH VIÊN ===
            model.TotalMembers = db.Accounts.Count();

            // === 5. C?NH BÁO SALE S?P H?T H?N (TRONG 3 NGÀY) ===
            var today = DateTime.Now;
            var expiringSales = db.Sales
                .Where(s => s.sale_end >= today && s.sale_end <= DbFunctions.AddDays(today, 3))
                .Include(s => s.Variant_Phone)
                .Include(s => s.Variant_Phone.Product)
                .OrderBy(s => s.sale_end)
                .ToList();

            model.ExpiringSales = expiringSales.Select(s => new ExpiringSaleAlert
            {
                SaleId = s.sale_id,
                SaleName = s.sale_name,
                SaleEnd = s.sale_end,
                ProductName = s.Variant_Phone.Product.product_name,
                VariantInfo = $"{s.Variant_Phone.variant_ph_color} {s.Variant_Phone.variant_ph_ram}/{s.Variant_Phone.variant_ph_rom}GB",
                DaysRemaining = (int)(s.sale_end - today).TotalDays
            }).ToList();

            // === 6. C?NH BÁO S?N PH?M S?P H?T HÀNG (T?N KHO < 5) ===
            var lowStockProducts = db.Stocks
                .Where(s => s.stock_count < 5)
                .Include(s => s.Variant_Phone)
                .Include(s => s.Variant_Phone.Product)
                .OrderBy(s => s.stock_count)
                .ToList();

            model.LowStockProducts = lowStockProducts.Select(s => new LowStockAlert
            {
                VariantId = s.variant_id,
                ProductName = s.Variant_Phone.Product.product_name,
                VariantColor = s.Variant_Phone.variant_ph_color,
                Ram = s.Variant_Phone.variant_ph_ram,
                Rom = s.Variant_Phone.variant_ph_rom,
                StockCount = s.stock_count,
                ProductImage = s.Variant_Phone.variant_ph_img
            }).ToList();

            // === 7. BI?U ?? DOANH THU 7 NGÀY G?N NH?T ===
            var sevenDaysAgo = today.AddDays(-6); // L?y t? 6 ngày tr??c + hôm nay = 7 ngày

            // L?y doanh thu theo ngày (ch? ??n ?ã giao - status = 3)
            var revenueData = db.Order_
                .Where(o => o.order_state == 3 && o.order_buy_time >= sevenDaysAgo)
                .GroupBy(o => DbFunctions.TruncateTime(o.order_buy_time))
                .Select(g => new
                {
                    Date = g.Key.Value,
                    Revenue = g.Sum(o => o.order_total_price)
                })
                .OrderBy(r => r.Date)
                .ToList();

            // T?o list 7 ngày ??y ?? (k? c? ngày không có doanh thu = 0)
            model.RevenueChart = Enumerable.Range(0, 7)
                .Select(offset => sevenDaysAgo.AddDays(offset).Date)
                .Select(date => new RevenueByDay
                {
                    Date = date,
                    Revenue = revenueData.FirstOrDefault(r => r.Date == date)?.Revenue ?? 0
                })
                .ToList();

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
