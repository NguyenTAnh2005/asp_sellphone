using old_phone.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using old_phone.Common;

namespace old_phone.Controllers.User
{
    public class CartsController : Controller
    {
        OldPhoneEntities db = new OldPhoneEntities();

        // GET: Giỏ hàng
        [AuthorizeCheck]
        public ActionResult Index()
        {
            var id_acc = Session["acc_id"] as int?;
            var listCartItems = db.Carts
                                  .Where(c => c.account_id == id_acc)
                                  .Include(c => c.Variant_Phone)
                                  .Include(c => c.Variant_Phone.Product)
                                  .Include(c => c.Variant_Phone.Product.Phone)
                                  .ToList();
            return View(listCartItems);
        }
        // --- CÁC HÀM API JSON (Xử lý ngầm) ---
        // Model nhận dữ liệu cập nhật số lượng
        public class CartUpdateModel
        {
            public int cart_id { get; set; }
            public int cart_count { get; set; }
        }
        // Cập nhật số lượng (JSON)
        [HttpPost]
        public JsonResult UpdateCountJS(List<CartUpdateModel> carts)
        {
            var acc_id = Session["acc_id"] as int?;
            if (acc_id == null)
            {
                return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập!" });
            }

            try
            {
                if (carts != null && carts.Any())
                {
                    foreach (var item in carts)
                    {
                        // Chỉ cập nhật cart item của đúng user đó
                        var cartitem = db.Carts.FirstOrDefault(c => c.cart_id == item.cart_id && c.account_id == acc_id);
                        if (cartitem != null && item.cart_count > 0)
                        {
                            cartitem.cart_count = item.cart_count;
                        }
                    }
                    db.SaveChanges();

                    return Json(new { success = true, message = "Cập nhật giỏ hàng thành công!" });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        //Xóa sản phẩm (JSON)
        [HttpPost]
        public JsonResult DeleteCartJS(int[] selectedItems)
        {
            var acc_id = Session["acc_id"] as int?;
            if (acc_id == null)
            {
                return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập!" });
            }

            try
            {
                if (selectedItems != null && selectedItems.Length > 0)
                {
                    var itemsToDelete = db.Carts
                        .Where(c => selectedItems.Contains(c.cart_id) && c.account_id == acc_id)
                        .ToList();

                    if (itemsToDelete.Any())
                    {
                        db.Carts.RemoveRange(itemsToDelete);
                        db.SaveChanges();
                        return Json(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng!" });
                    }
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm để xóa!" });
                }
                return Json(new { success = false, message = "Vui lòng chọn sản phẩm cần xóa!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        //Thêm vào giỏ hàng (JSON) từ trang chi tiết sản phẩm
        [HttpPost]
        public JsonResult AddToCartJS(int variant_id)
        {
            var acc_id = Session["acc_id"] as int?;
            if (acc_id == null)
            {
                return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập để mua hàng!" });
            }

            try
            {
                var cartItem = db.Carts.FirstOrDefault(c => c.account_id == acc_id && c.variant_id == variant_id);
                string msg = "";

                if (cartItem != null)
                {
                    cartItem.cart_count += 1;
                    msg = "Đã cập nhật số lượng sản phẩm trong giỏ!";
                }
                else
                {
                    var newCart = new Cart
                    {
                        account_id = acc_id.Value,
                        variant_id = variant_id,
                        cart_count = 1
                    };
                    db.Carts.Add(newCart);
                    msg = "Đã thêm sản phẩm mới vào giỏ hàng!";
                }
                db.SaveChanges();

                // Đếm tổng số lượng item để update badge trên header
                int totalCount = db.Carts.Count(c => c.account_id == acc_id);

                return Json(new { success = true, message = msg, totalCartItem = totalCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // Chuyển đến trang Thanh toán từ Giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck]
        public ActionResult ProceedToCheckout(List<int> selectedItems)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                TempData["Message"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán!";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }

            TempData["SelectedItems"] = selectedItems;
            return RedirectToAction("Checkout", "Order");
        }

        //Mua ngay từ trang Chi tiết sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck]
        public ActionResult BuyNow(int variant_id, int quantity = 1)
        {
            TempData["BuyNow_ID"] = variant_id;
            TempData["BuyNow_Qty"] = quantity;
            return RedirectToAction("Checkout", "Order");
        }
    }
}