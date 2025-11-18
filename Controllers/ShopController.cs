using old_phone.Models; // Cần để dùng .Include
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using old_phone.ViewModels;

namespace old_phone.Controllers
{
    public class ShopController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        public ActionResult Index()
        {
            // 1. Lấy sản phẩm HOT SALE (Có giá mới < giá cũ, hoặc nằm trong bảng Sale)
            var hotSales = db.Variant_Phone
                             .Include(v => v.Product)
                             .OrderBy(v => v.variant_ph_new_price)
                             .Take(6)
                             .ToList();

            // 2. Lấy sản phẩm NỔI BẬT (Ví dụ lấy các dòng máy đắt tiền hoặc mới nhất)
            var featured = db.Variant_Phone
                             .Include(v => v.Product)
                             .OrderByDescending(v => v.variant_id) // Lấy máy mới nhập
                             .Take(12)
                             .ToList();

            // 3. Lấy Blog mới nhất
            var blogs = db.Blogs
                          .OrderByDescending(b => b.blog_time)
                          .Take(5)
                          .ToList();

            var model = new HomepageViewModel
            {
                HotSales = hotSales,
                FeaturedPhones = featured,
                Blogs = blogs
            };

            return View(model);
        }

        public ActionResult Phones()
        {
            return View("Phones");
        }

        // GET: ProductDetails
        public ActionResult ProductDetails(int id)
        {
            var variant = db.Variant_Phone
                .Include("Product") // Lấy kèm thêm product và Phone thông qua product để tránh truy vấn thêm 
                .Include("Product.Phone")
                .FirstOrDefault(v => v.variant_id == id);

            if (variant == null) return HttpNotFound();

            // Lấy tồn kho 

            var stock = db.Stocks.FirstOrDefault(s => s.variant_id == id);
            int stock_count = (stock == null) ? 0 : stock.stock_count;

            // Laays danh sach anh bo vao carousel

            var list_product_image = db.Product_Image
                .Where(i => i.product_id == variant.product_id)
                .Select(i => i.image_url)
                .ToList();

            var model = new ProductDetailsViewModel
            {
                Product = variant.Product,
                Variant = variant,
                Phone = variant.Product.Phone,
                StockCount = stock_count,
                ListImages = list_product_image
            };

            return View( model);
        }
    }
}