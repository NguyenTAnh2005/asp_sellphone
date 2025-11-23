using old_phone.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using old_phone.Common;

namespace old_phone.Controllers.User
{
    public class CartsController : Controller
    {
        OldPhoneEntities db = new OldPhoneEntities();
        // GET: 
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

        // Xu ly cap nhat So luong 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCount(List<Cart> carts)
        {
            // MVC tự động gom các input name="carts[0].cart_id"... thành List<Cart>
            if(carts != null)
            {
                foreach(var item in carts)
                {
                    var cartitem = db.Carts.Find(item.cart_id);
                    if(cartitem != null)
                    {
                        cartitem.cart_count = item.cart_count;
                    }
                }
                db.SaveChanges();
                TempData["Message"] = "Cập nhật số lượng thành công!";
                TempData["MsgType"] = "success";
            }
            return RedirectToAction("Index");
        }

        // XU ly xoa cart item 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCart(int[] selectedItems)
        {
            if(selectedItems != null && selectedItems.Length > 0)
            {
                // Chon tat cac cac item ma cart_id nam trong danh sach dau vao ben tren 
                var itemCartToDel = db.Carts.Where(c => selectedItems.Contains(c.cart_id)).ToList();
                //SELECT* FROM Cart WHERE cart_id IN(1, 5, 9, 12)
                db.Carts.RemoveRange(itemCartToDel);
                db.SaveChanges();

            }
            TempData["Message"] = "Xóa sản phẩm  thành công!";
            TempData["MsgType"] = "success";
            return RedirectToAction("Index");
        }

        // Add CartItem tu trang chi tiet san pham 
        [HttpPost]
        public  JsonResult AddToCartJS(int variant_id)
        {
            var acc_id = Session["acc_id"] as int?;
            if (acc_id == null)
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true, // Cờ hiệu để JS biết mà chuyển trang
                    message = "Vui lòng đăng nhập để thực hiện chức năng này!"
                });
            }
            try
            {
                var cart_item = db.Carts.FirstOrDefault(c => c.account_id == acc_id && c.variant_id == variant_id);
                string msg = "";

                if (cart_item != null)
                {
                    cart_item.cart_count += 1;
                    msg = "Đã cập nhật số lượng sản phẩm!";
                }
                else
                {
                    Cart newCartItem = new Cart();
                    newCartItem.account_id = acc_id.Value;
                    newCartItem.variant_id = variant_id;
                    newCartItem.cart_count = 1;
                    db.Carts.Add(newCartItem);
                    msg = "Đã thêm mới vào giỏ hàng!";
                }
                db.SaveChanges();

                // Đếm tổng số lượng sản phẩm trong giỏ (để cập nhật số trên icon giỏ hàng)
                int totalCount = db.Carts.Where(c => c.account_id == acc_id).Count(); // Hoặc Sum(cart_count) tùy bạn

                // Trả về kết quả thành công
                return Json(new { success = true, message = msg, totalCartItem = totalCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // Nhan danh sach cart item duoc chon de mua => chuyen den trang Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck]
        public ActionResult ProceedToCheckout(List<int> selectedItems)
        {
            // 1. Kiểm tra nếu không có sản phẩm nào được chọn
            if (selectedItems == null || !selectedItems.Any())
            {
                TempData["Message"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán!";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }

            // 2. Lưu danh sách ID đã chọn vào TempData
            TempData["SelectedItems"] = selectedItems;

            // 3. Chuyển hướng sang trang Thanh toán chính (Order/Checkout)
            return RedirectToAction("Checkout", "Order");
        }

        // Nhan thong tin sp mua ngay o ben trang chi tiet sp
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck]

        public ActionResult BuyNow(int variant_id, int quantity = 1)
        {
            // 1. Lưu trực tiếp ID và Số lượng
            TempData["BuyNow_ID"] = variant_id;
            TempData["BuyNow_Qty"] = quantity;

            // 2. Chuyển hướng
            return RedirectToAction("Checkout", "Order");
        }
    }
}