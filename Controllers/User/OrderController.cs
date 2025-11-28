using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using old_phone.Common;
using old_phone.Models;
using old_phone.ViewModels;

namespace old_phone.Controllers.User
{
    public class OrderController : Controller
    {
        // GET: Order
        private OldPhoneEntities db = new OldPhoneEntities();
        [AuthorizeCheck]
        public ActionResult Checkout()
        {
            var acc_id = Convert.ToInt32(Session["acc_id"]);
            List<OrderItemViewModel> orderItems = new List<OrderItemViewModel>();
            var listHotline = db.Hotlines.Where(h => h.account_id == acc_id).OrderByDescending(h => h.hotline_default).ToList();
            // --- KIỂM TRA: CÓ ĐỊA CHỈ TRONG SỔ ĐỊA CHỈ CHƯA ---
            if(listHotline.Count == 0)
            {
                TempData["Message"] = "Vui lòng thêm địa chỉ nhận hàng trước khi thanh toán!";
                TempData["MsgType"] = "error";
                return RedirectToAction("Index", "Hotline");
            }
            // --- KIỂM TRA: CÓ PHẢI MUA NGAY KHÔNG? ---
            if (TempData["BuyNow_ID"] != null)
            {
                // 1. Kiểm tra có phải MUA NGAY ko (Lấy từ TempData -> Query bảng Variant)
                int v_id = (int)TempData["BuyNow_ID"];
                int qty = (int)TempData["BuyNow_Qty"];

                // Giữ lại để dùng cho bước Thanh toán sau
                TempData.Keep("BuyNow_ID");
                TempData.Keep("BuyNow_Qty");

                var variant = db.Variant_Phone.Include("Product").FirstOrDefault(v => v.variant_id == v_id);
                if (variant != null)
                {
                    orderItems.Add(new OrderItemViewModel
                    {
                        variant_id = variant.variant_id,
                        name = variant.Product.product_name,
                        ram = variant.variant_ph_ram,
                        rom = variant.variant_ph_rom,
                        price = variant.variant_ph_final_price,
                        count = qty, // <-- Lấy số lượng mua ngay (mặc định là 1)
                        image_url = variant.variant_ph_img
                    });
                }
            }
            else
            {
                // Nếu như ko phải mua ngay => chắc chẳn đang mua từ giỏ hàng 
                var selectedItems = TempData["SelectedItems"] as List<int>;
                if (selectedItems != null && selectedItems.Any())
                {
                    TempData.Keep("SelectedItems"); // Giữ lại
                    var cartItems = db.Carts.Where(c => c.account_id == acc_id && selectedItems.Contains(c.cart_id)).ToList();

                    orderItems = cartItems.Select(c => new OrderItemViewModel
                    {
                        variant_id = c.variant_id,
                        name = c.Variant_Phone.Product.product_name,
                        ram = c.Variant_Phone.variant_ph_ram,
                        rom = c.Variant_Phone.variant_ph_rom,
                        price = c.Variant_Phone.variant_ph_final_price,
                        count = c.cart_count, // <-- Lấy số lượng trong giỏ
                        image_url = c.Variant_Phone.variant_ph_img
                    }).ToList();
                }
            }
            if (orderItems.Count == 0) return RedirectToAction("Index", "Carts"); // Nếu rỗng thì đá về giỏ

            return View(new OrderViewModel { OrderItems = orderItems, listHotlines = listHotline });
        }

        // POST: Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(OrderViewModel model)
        {
            var acc_id = Convert.ToInt32(Session["acc_id"]);
            // Danh sách các món sẽ mua để tạo Detail
            // Dùng một class tạm hoặc tái sử dụng OrderItemViewModel để lưu thông tin cần thiết (ID, Giá, Số lượng)
            var itemsToCheckout = new List<OrderItemViewModel>();

            // Cờ đánh dấu: Có cần xóa giỏ hàng sau khi mua không?
            bool needClearCart = false;

            // --- BƯỚC 1: XÁC ĐỊNH NGUỒN HÀNG ---
            if (TempData["BuyNow_ID"] != null)
            {
                // A. TRƯỜNG HỢP MUA NGAY
                int v_id = (int)TempData["BuyNow_ID"];
                int qty = (int)TempData["BuyNow_Qty"];

                var variant = db.Variant_Phone
                    .Include("Product")
                    .FirstOrDefault(v => v.variant_id == v_id);
                if (variant != null)
                {
                    itemsToCheckout.Add(new OrderItemViewModel
                    {
                        variant_id = v_id,
                        price = variant.variant_ph_final_price,
                        count = qty,
                        name = variant.Product.product_name // Lưu tên để add vào detail
                    });
                }

                // Mua ngay -> KHÔNG xóa giỏ hàng
                needClearCart = false;
            }
            else
            {
                // B. TRƯỜNG HỢP MUA TỪ GIỎ
                var selectedItems = TempData["SelectedItems"] as List<int>;
                if (selectedItems != null)
                {
                    var cartData = db.Carts.Where(c => c.account_id == acc_id && selectedItems.Contains(c.cart_id)).ToList();

                    // Chuyển sang list chung
                    foreach (var c in cartData)
                    {
                        itemsToCheckout.Add(new OrderItemViewModel
                        {
                            variant_id = c.variant_id,
                            price = c.Variant_Phone.variant_ph_final_price,
                            count = c.cart_count,
                            name = c.Variant_Phone.Product.product_name
                        });
                    }

                    // Mua từ giỏ -> CẦN xóa giỏ hàng ở dưới 
                    needClearCart = true;
                }
            }

            if (itemsToCheckout.Count == 0) return RedirectToAction("Index", "Shop");

            // --- BƯỚC 2: TẠO ORDER (GIỮ NGUYÊN) ---
            var newOrder = new Order_();
            newOrder.account_id = acc_id;
            newOrder.hotline_id = model.Order_HotlineId;
            newOrder.order_buy_time = DateTime.Now;
            newOrder.order_rec_time = DateTime.Now.AddDays(3);
            newOrder.order_state = 0; // mặc định của SQL đã là 0 r nhưng thêm cho chắc
            newOrder.order_type_pay = model.Order_TypePayment;
            newOrder.order_total_price = (long)itemsToCheckout.Sum(x => x.price * x.count);

            db.Order_.Add(newOrder);
            db.SaveChanges();

            // --- BƯỚC 3: TẠO DETAIL & TRỪ KHO & XÓA GIỎ (NẾU CẦN) ---
            foreach (var item in itemsToCheckout)
            {
                // 3.1 Lưu Detail
                var detail = new Detail();
                detail.order_id = newOrder.order_id;
                detail.variant_id = item.variant_id;
                detail.detail_name = item.name;
                detail.detail_count = item.count;
                detail.detail_total_price = (long)item.price;
                db.Details.Add(detail);

                // 3.2 Trừ kho
                var stock = db.Stocks.FirstOrDefault(s => s.variant_id == item.variant_id);
                if (stock != null) stock.stock_count -= item.count;

                // 3.3 Xóa giỏ hàng (CHỈ CHẠY NẾU LÀ MUA TỪ GIỎ)
                if (needClearCart)
                {
                    // Tìm item trong giỏ để xóa
                    var cartItem = db.Carts.FirstOrDefault(c => c.account_id == acc_id && c.variant_id == item.variant_id);
                    if (cartItem != null) db.Carts.Remove(cartItem);
                }
            }

            db.SaveChanges();

            // Dọn dẹp sạch TempData
            TempData.Remove("BuyNow_ID");
            TempData.Remove("BuyNow_Qty");
            TempData.Remove("SelectedItems");

            return View("Success");
        }

        // Xem lịch sử đơn hàng
        // GET: Order/History
        [AuthorizeCheck]
        public ActionResult History(int? status)
        {
            var acc_id = Convert.ToInt32(Session["acc_id"]);

            // 1. Lấy đơn hàng của user
            var ordersQuery = db.Order_
                                .Where(o => o.account_id == acc_id)
                                .OrderByDescending(o => o.order_buy_time)
                                .AsQueryable();

            // 2. Lọc theo trạng thái (Quy ước: -1 là Xem Tất Cả)
            if (status.HasValue && status.Value != -1)
            {
                ordersQuery = ordersQuery.Where(o => o.order_state == status.Value);
            }

            // 3. Mapping dữ liệu sang ViewModel (Giữ nguyên logic cũ của bạn)
            var historyList = new List<OrderHistoryViewModel>();
            foreach (var order in ordersQuery.ToList())
            {
                var details = db.Details.Where(d => d.order_id == order.order_id).ToList();
                var itemNames = details.Take(3).Select(d => d.detail_name).ToList();

                historyList.Add(new OrderHistoryViewModel
                {
                    order_id = order.order_id,
                    order_buy_time = order.order_buy_time,
                    // Lưu ý: ViewModel nên để int, nếu là string thì ép kiểu tạm
                    order_state = order.order_state.Value,
                    order_total_price = order.order_total_price,
                    order_type_pay = order.order_type_pay,
                    ItemNames = itemNames,
                    TotalItemCount = details.Sum(d => d.detail_count)
                });
            }

            // 4. TẠO DANH SÁCH DROPDOWN TRẠNG THÁI (MỚI)
            var statusItems = new List<SelectListItem>
            {
                new SelectListItem { Value = "-1", Text = "--- Tất cả đơn hàng ---" },
                new SelectListItem { Value = "0", Text = "Đang xác nhận" },
                new SelectListItem { Value = "1", Text = "Đang chuẩn bị hàng" },
                new SelectListItem { Value = "2", Text = "Đang giao hàng" },
                new SelectListItem { Value = "3", Text = "Giao thành công" },
                new SelectListItem { Value = "4", Text = "Đã hủy" }
            };

            // Chọn sẵn giá trị đang lọc (nếu null thì chọn -1)
            ViewBag.StatusList = new SelectList(statusItems, "Value", "Text", status ?? -1);

            return View(historyList);
        }

        // Xem chi tiết đơn hàng
        [AuthorizeCheck]
        [HttpGet]
        public ActionResult Details(int id)
        {
            var acc_id = Convert.ToInt32(Session["acc_id"]);

            // 1. Tìm Order, Hotline và Details. Đảm bảo Order thuộc về người dùng đang đăng nhập.
            var order = db.Order_
                            .Include("Hotline") // Bắt buộc phải có Include để lấy thông tin địa chỉ
                            .FirstOrDefault(o => o.order_id == id && o.account_id == acc_id);

            if (order == null)
            {
                TempData["Message"] = "Không tìm thấy hóa đơn hoặc bạn không có quyền truy cập.";
                TempData["MsgType"] = "error";
                return RedirectToAction("History");
            }

            // Lấy chi tiết sản phẩm (Details)
            var details = db.Details
                            .Where(d => d.order_id == order.order_id)
                            .ToList();

            // 2. Mapping dữ liệu sang OrderDetailViewModel
            var viewModel = new OrderDetailViewModel
            {
                OrderId = order.order_id,
                BuyTime = order.order_buy_time,
                State = order.order_state.Value,
                TotalPrice = order.order_total_price,
                PaymentType = order.order_type_pay,

                // Thông tin địa chỉ từ Hotline
                RecipientName = order.Hotline.hotline_name,
                RecipientPhone = order.Hotline.hotline_phonenumber,
                RecipientAddress = order.Hotline.hotline_address,

                // Mapping danh sách sản phẩm
                Items = details.Select(d => new OrderDetailItem
                {
                    ProductName = d.detail_name,
                    Quantity = d.detail_count,
                    UnitPrice = d.detail_total_price,
                    // Lấy URL ảnh từ bảng Variant_Phone (cần JOIN hoặc SELECT riêng)
                    // Giả định Detail có variant_id, ta query ảnh từ Variant_Phone
                    ImageUrl = db.Variant_Phone.FirstOrDefault(v => v.variant_id == d.variant_id).variant_ph_img}).ToList()
            };
            // --- LOGIC MỚI: Xử lý trạng thái ---
            switch (viewModel.State)
            {
                case 0:
                    viewModel.StateName = "Đang Chờ Xác Nhận";
                    viewModel.StateClass = "text-warning"; // Màu vàng
                    break;
                case 1:
                    viewModel.StateName = "Đang Chuẩn Bị Hàng";
                    viewModel.StateClass = "text-info"; // Màu xanh dương nhạt
                    break;
                case 2:
                    viewModel.StateName = "Đang Giao Hàng";
                    viewModel.StateClass = "text-primary"; // Màu xanh dương đậm
                    break;
                case 3:
                    viewModel.StateName = "Giao Thành Công";
                    viewModel.StateClass = "text-success"; // Màu xanh lá
                    break;
                case 4:
                    viewModel.StateName = "Đã Hủy";
                    viewModel.StateClass = "text-danger"; // Màu đỏ
                    break;
                default:
                    viewModel.StateName = "Không Xác Định";
                    viewModel.StateClass = "text-secondary";
                    break;
            }

            return View(viewModel);
        }
    }
}