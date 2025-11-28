using old_phone.Common;
using old_phone.Models;
using old_phone.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace old_phone.Controllers.Manage
{
    public class ManageOrder_ViewModelController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: Manage/ManageOrder_
        [AuthorizeCheck(RequiredRole = 2)] 
        public ActionResult Index(int? order_State,DateTime? date,int?page)
        {
            // 1. Khởi tạo truy vấn (Chưa chạy SQL ngay)
            var ordersQuery = db.Order_
                .Include(o => o.Hotline)
                .Include(o => o.Account)
                .AsQueryable(); // Quan trọng: Giữ IQueryable để lọc SQL

            // 2. Lọc theo trạng thái (Nếu có chọn)
            if (order_State.HasValue && order_State.Value != -1) // Giả sử -1 là "Tất cả"
            {
                ordersQuery = ordersQuery.Where(o => o.order_state == order_State.Value);
            }

            // 3. Lọc theo ngày (Nếu có chọn)
            if (date.HasValue)
            {
                // QUAN TRỌNG: Tách giá trị ngày ra biến riêng bên ngoài
                // Để EF hiểu đây là một giá trị hằng số (Parameter), không phải lệnh SQL
                var filterDate = date.Value.Date;

                // Sử dụng DbFunctions.TruncateTime để cắt giờ trong Database và so sánh
                ordersQuery = ordersQuery.Where(o => DbFunctions.TruncateTime(o.order_buy_time) == filterDate);
            }


            // 4. Sắp xếp (Mới nhất lên đầu)
            ordersQuery = ordersQuery.OrderByDescending(o => o.order_buy_time);

            // 5. Mapping sang ViewModel
            // Lưu ý: Phải ToList() ở đây để lấy dữ liệu về RAM rồi mới foreach tạo ViewModel
            // (Vì AdminOrderHeaderViewModel không phải Entity nên không dùng .Select trực tiếp được dễ dàng)
            var ordersAll = ordersQuery.ToList();
            var viewModels = new List<AdminOrderHeaderViewModel>();

            // Chuyển từ Order_ sang ViewModel AdminOrderHeaderViewModel - item
            // được show trong  danh sách đơn hàng 

            foreach (var order in ordersAll)
            {
                var vm = new AdminOrderHeaderViewModel();
                vm.OrderId = order.order_id;
                vm.OrderBuyTime = order.order_buy_time;
                vm.OrderTotalPrice = order.order_total_price;
                vm.OrderState = order.order_state ?? 0;
                vm.CustomerId = order.account_id.Value;
                if (order.Account != null)
                {
                    vm.CustomerFullName = order.Account.account_last_name + " " + order.Account.account_first_name;
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
            // 6. Phân trang cho List ViewModel
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            // Lưu ViewBag để giữ giá trị bộ lọc trên View
            ViewBag.CurrentFilterState = order_State;
            // Format ngày yyyy-MM-dd để input type="date" hiển thị đúng
            ViewBag.CurrentFilterDate = date.HasValue ? date.Value.ToString("yyyy-MM-dd") : "";

            // Tạo Dropdown trạng thái
            var statusItems = new List<SelectListItem>
            {
                new SelectListItem { Value = "-1", Text = "--- Tất cả trạng thái ---" },
                new SelectListItem { Value = "0", Text = "Đang xác nhận" },
                new SelectListItem { Value = "1", Text = "Đang chuẩn bị" },
                new SelectListItem { Value = "2", Text = "Đang giao hàng" },
                new SelectListItem { Value = "3", Text = "Giao thành công" },
                new SelectListItem { Value = "4", Text = "Đã hủy" }
            };
            ViewBag.StatusList = new SelectList(statusItems, "Value", "Text", order_State ?? -1);

            // Trả về PagedList
            return View(viewModels.ToPagedList(pageNumber, pageSize));
        }

        // GET: ManageOrder_/Details/5
        [AuthorizeCheck(RequiredRole = 2)]
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

        // POST: Cập nhật trạng thái đơn hàng từ trang Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)] 
        public ActionResult UpdateStatus(int OrderId, int SelectedStatus)
        {
            // 1. Tìm đơn hàng theo ID
            var order = db.Order_.Find(OrderId);

            if (order != null)
            {
                // 2. Cập nhật trạng thái mới
                order.order_state = SelectedStatus;

                // 3. Logic phụ: Nếu trạng thái là "Giao thành công" (3) -> Cập nhật ngày nhận hàng thực tế
                if (SelectedStatus == 3)
                {
                    order.order_rec_time = DateTime.Now;
                }

                // 4. Lưu vào Database
                db.SaveChanges();

                // 5. Thông báo thành công (dùng TempData để hiển thị ở trang Details sau khi redirect)
                TempData["Message"] = "Cập nhật trạng thái đơn hàng thành công!";
                TempData["MsgType"] = "success";

                var account_email = db.Accounts.Find(order.account_id)?.account_email;
                if (account_email != null)
                {
                    // Gửi email thông báo cho khách hàng về việc cập nhật trạng thái đơn hàng
                    var account = db.Accounts.Find(order.account_id);
                    var account_full_name = account.account_last_name + " " + account.account_first_name;
                    string subject = "[Old Phone]Cập nhật trạng thái đơn hàng của bạn";
                    string body = $"Chào {account_full_name},<br/><br/>" +
                                      $"Đơn hàng có mã số <b style='color:red; font-size:18px;'>#{OrderId}</b>  của bạn đã được cập nhật trạng thái mới:<br/><br/>" +
                                      "<b style='color:lime; font-size:14px;'>";
                    switch (SelectedStatus)
                    {
                        case 0: body += "Đang xác nhận</b>"; break;
                        case 1: body += "Đang chuẩn bị</b>"; break;
                        case 2: body += "Đang giao hàng</b>"; break;
                        case 3: body += "Giao thành công</b>"; break;
                        case 4: body += "Đã hủy</b>"; break;
                        default: body += "Không xác định</b>"; break;
                    }
                    body += "<br/><br/>Bạn có thể kiểm tra lại trên trang web";
                    body += "<br/><br/>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!";

                    MailHelper.SendEmail(account_email, subject, body);
                }
            }
            else
            {
                TempData["Message"] = "Không tìm thấy đơn hàng!";
                TempData["MsgType"] = "error";
            }

            // 6. Quay lại trang chi tiết của đơn hàng đó
            return RedirectToAction("Index");
        }

    }
}