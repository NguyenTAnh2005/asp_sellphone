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

            var selectedItems = TempData["SelectedItems"] as List<int>;
            var listHotline = db.Hotlines.Where(h => h.account_id == acc_id)
                                         .OrderByDescending(h => h.hotline_default).ToList();
            var cartItems = db.Carts
                .Where(c => c.account_id == acc_id && selectedItems.Contains(c.cart_id))
                .ToList();

            if(cartItems.Count == 0)
            {
                var cartItemFromVariant = db.Variant_Phone.Where(v => selectedItems.Contains(v.variant_id));
                var orderItemsFromVariant = cartItemFromVariant.Select(c => new OrderItemViewModel
                {
                    variant_id = c.variant_id,
                    name = c.Product.product_name,
                    ram = c.variant_ph_ram,
                    rom = c.variant_ph_rom,
                    price = c.variant_ph_final_price,
                    count = 1,
                    image_url = c.variant_ph_img
                }).ToList();
                var orderViewModelFormVariant = new OrderViewModel
                {
                    OrderItems = orderItemsFromVariant,
                    listHotlines = listHotline
                };
                return View(orderViewModelFormVariant);
            }
            List<OrderItemViewModel> orderItems = cartItems.Select(c => new OrderItemViewModel
            {
                variant_id = c.variant_id,
                name = c.Variant_Phone.Product.product_name,
                ram = c.Variant_Phone.variant_ph_ram,
                rom = c.Variant_Phone.variant_ph_rom,
                price = c.Variant_Phone.variant_ph_final_price,
                count = c.cart_count,
                image_url = c.Variant_Phone.variant_ph_img
            }).ToList();
            var orderViewModel = new OrderViewModel
            {
                OrderItems = orderItems,
                listHotlines = listHotline
            };
            return View(orderViewModel);

        }
    }
}