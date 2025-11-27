using old_phone.Common;
using old_phone.Models;
using old_phone.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace old_phone.Controllers.Manage
{
    public class ManageOrder_ViewModelController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: Manage/ManageOrder_
        public ActionResult Index()
        {
            // Truy vấn DL Order có  kèm hotline và account 
            var ordersAll = db.Order_
                .Include(o => o.Hotline)
                .Include(o => o.Account)
                .OrderByDescending(o => o.order_buy_time)
                .ToList();
            // Chuyển từ Order_ sang ViewModel AdminOrderHeaderViewModel - item
            // được show trong  danh sách đơn hàng 
            var viewModels = new List<AdminOrderHeaderViewModel>();

            foreach (var order in ordersAll)
            {
                var vm = new AdminOrderHeaderViewModel();
                vm.OrderId = order.order_id;
                vm.OrderBuyTime = order.order_buy_time;
                vm.OrderTotalPrice = order.order_total_price;
                vm.OrderState = order.order_state ?? 0;
                if (order.Account != null)
                {
                    vm.CustomerFullName = order.Account.account_last_name + " " + order.Account.account_last_name;
                }
                else
                {
                    ViewBag.Error = "Tài khoản khách hàng đã bị xóa!";
                }
                if (order.Hotline != null)
                {
                    vm.RecipientPhoneNumber = order.Hotline.hotline_phonenumber;
                }
                else
                {
                    ViewBag.Error = "Số điện thoại liên hệ đã bị xóa!";
                }
                switch (vm.OrderState)
                {
                    case 0: vm.OrderStateDisplay = "Đang xác nhận"; break;
                    case 1: vm.OrderStateDisplay = "Đang chuẩn bị"; break;
                    case 2: vm.OrderStateDisplay = "Đang giao"; break;
                    case 3: vm.OrderStateDisplay = "Đã giao"; break;
                    case 4: vm.OrderStateDisplay = "Đã hủy"; break;
                    default: vm.OrderStateDisplay = "Chưa rõ"; break;
                }

                viewModels.Add(vm);
            }

            return View(viewModels);
        }

        // GET: ManageOrder_/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // 1. Truy vấn đơn hàng và các bảng liên quan
            var order = db.Order_
                .Include(o => o.Hotline)
                .Include(o => o.Account)
                .Include(o => o.Details.Select(d => d.Variant_Phone.Product)) // Include sâu để lấy tên Product
                .FirstOrDefault(o => o.order_id == id);

            if (order == null) return HttpNotFound();

            // 2. Khởi tạo ViewModel tổng
            var model = new ManageOrder_ViewModel();

            // --- A. MAP HEADER (Thông tin chung) ---
            model.Header = new AdminOrderHeaderViewModel
            {
                OrderId = order.order_id,
                OrderBuyTime = order.order_buy_time,
                OrderTotalPrice = order.order_total_price,
                OrderState = order.order_state ?? 0,

                // Xử lý null nếu tài khoản hoặc hotline đã bị xóa
                CustomerFullName = (order.Account != null)
                    ? order.Account.account_last_name + " " + order.Account.account_first_name
                    : "Khách vãng lai / Đã xóa",

                RecipientPhoneNumber = (order.Hotline != null)
                    ? order.Hotline.hotline_phonenumber
                    : "Không có SĐT"
            };

            // Map trạng thái hiển thị
            switch (model.Header.OrderState)
            {
                case 0: model.Header.OrderStateDisplay = "Đang xác nhận"; break;
                case 1: model.Header.OrderStateDisplay = "Đang chuẩn bị"; break;
                case 2: model.Header.OrderStateDisplay = "Đang giao hàng"; break;
                case 3: model.Header.OrderStateDisplay = "Giao thành công"; break;
                case 4: model.Header.OrderStateDisplay = "Đã hủy"; break;
                default: model.Header.OrderStateDisplay = "Không xác định"; break;
            }

            // --- B. MAP THÔNG TIN BỔ SUNG ---
            model.ShippingAddress = (order.Hotline != null) ? order.Hotline.hotline_address : "Địa chỉ không tồn tại";
            model.PaymentMethod = order.order_type_pay; // "COD" hoặc "Banking"

            // --- C. MAP DANH SÁCH SẢN PHẨM (ITEMS) ---
            model.Items = order.Details.Select(d => new OrderDetailItemViewModel
            {
                // Tên sản phẩm gốc
                MainProductName = d.Variant_Phone.Product.product_name,
                // Tên chi tiết trong đơn hàng
                ProductName = d.detail_name,
                Quantity = d.detail_count,
                ItemTotalPrice = d.detail_total_price,

                // Thông tin cấu hình
                Ram = d.Variant_Phone.variant_ph_ram,
                Rom = d.Variant_Phone.variant_ph_rom,
                Color = d.Variant_Phone.variant_ph_color
            }).ToList();

            // --- D. MAP DANH SÁCH TRẠNG THÁI (STATUS LIST) ---
            // Tạo danh sách để Admin chọn trong Dropdown
            model.StatusList = new List<StatusOption>
                {
                    new StatusOption { Value = 0, Text = "Đang xác nhận" },
                    new StatusOption { Value = 1, Text = "Đang chuẩn bị" },
                    new StatusOption { Value = 2, Text = "Đang giao hàng" },
                    new StatusOption { Value = 3, Text = "Giao thành công" },
                    new StatusOption { Value = 4, Text = "Đã hủy" }
                };
            model.SelectedStatus = model.Header.OrderState; // Gán trạng thái hiện tại vào biến SelectedStatus

            foreach (var s in model.StatusList)
            {
                // So sánh với biến SelectedStatus vừa gán
                if (s.Value == model.SelectedStatus)
                {
                    s.IsSelected = true;
                }
            }
            return View(model);
        }
    }
}