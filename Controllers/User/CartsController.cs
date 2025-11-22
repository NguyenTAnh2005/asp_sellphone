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
        [AuthorizeCheck]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int variant_id)
        {
            var account_id = Session["acc_id"] as int?;
            var cart_item = db.Carts.FirstOrDefault(c => c.account_id == account_id && c.variant_id == variant_id);
            if (cart_item != null)
            {
                cart_item.cart_count += 1;
                db.SaveChanges();
                TempData["Message"] = "Sản phẩm đã có sẵn trong giỏ hàng!!";
                TempData["MsgType"] = "info";
                return RedirectToAction("ProductDetails", "Shop", new { id = variant_id });
            }
            Cart newCartItem = new Cart();
            newCartItem.account_id = account_id.Value;
            newCartItem.variant_id = variant_id;
            newCartItem.cart_count = 1;
            db.Carts.Add(newCartItem);
            db.SaveChanges();
            TempData["Message"] = "Đã thêm sản phẩm vào giỏ hàng!!";
            TempData["MsgType"] = "success";
            return RedirectToAction("ProductDetails", "Shop", new { id = variant_id });
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

        public ActionResult BuyNow(int variant_id)
        {
            // Luu thong tin sp can mua ngay vao TempData
            TempData["SelectedItems"] = new List<int> { variant_id };
            // Chuyen huong den trang Checkout
            return RedirectToAction("Checkout", "Order");
        }
    }
}