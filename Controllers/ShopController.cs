using old_phone.Models; // Cần để dùng .Include
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using old_phone.ViewModels;
using PagedList;

namespace old_phone.Controllers
{
    public class ShopController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();
        //TRa ve view cua HOme 
        public ActionResult Index()
        {
            if (Request.Cookies["KeepLogin"] != null)
            {
                // Lays token de kiem tra trong db 
                var token = Request.Cookies["KeepLogin"].Values["token"];
                var account = db.Accounts.FirstOrDefault(a => a.RememberMeToken == token);
                // Neu nhu co token do trong DB va con han su dung 
                if (account != null && account.TokenExpiryDate > DateTime.Now)
                {
                    // Phuc hoi session 
                    Session["account"] = account;
                    Session["acc_id"] = account.account_id;
                    Session["acc_name"] = account.account_last_name + " " + account.account_first_name;
                    Session["acc_role"] = account.role_id;
                }
                else
                {
                    // Phong TH khi dang nhap may tinh, sau do dang nhap them va ghi nho dang nhap o thiet bi khac
                    // se tao ra token moi => ghi de vao database, thi o tren trinh duyet may tinh cookie luc nay co
                    // gia tri token khac voi token trong DB, nen neu nhu truy cap lai tren may tinh, sever se kiem tra token de cho dang nhap
                    // nhung luc nay token tren may tinh da khac so voi trong db do do  cookie tren may tinh se bi bo 
                    // bang cach tao 1 cookie trung ten nhung set han expried la ngay hom qua => trinh duyet check => xoa Cookie
                    var expriedCookie = new HttpCookie("KeepLogin");
                    expriedCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(expriedCookie);
                }
            }
            //1.Lấy sản phẩm HOT SALE
            var hotSales = db.Sales
                            .Where(s => s.sale_start <= DateTime.Now && DateTime.Now <= s.sale_end)

                            // Include bảng Variant và Product để dùng trong View
                            .Include(s => s.Variant_Phone)          // Lấy thông tin biến thể
                            .Include(s => s.Variant_Phone.Product)  // Lấy thông tin tên máy gốc

                            .OrderBy(s => s.Variant_Phone.variant_ph_final_price)
                            .Take(6)
                            .ToList();

            // 2. Lấy sản phẩm NỔI BẬT (Ví dụ lấy các dòng máy đắt tiền hoặc mới nhất)
            var featured = db.Variant_Phone
                             .Where(v => v.variant_ph_new_price==v.variant_ph_final_price)
                             .Include(v => v.Product)
                             .OrderBy(v => v.variant_id) // Lấy máy mới nhập
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

        // Tra ve view chi tiet sp
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

        // Tra ve du lieu cho trang PHONES
        public ActionResult Phones
            (string searchQuery, int? companyId, int? minPrice, int? maxPrice,
            string sort, int? ram, int? rom, string os, int? page)
        {
            // Truy vans chung nhat, queryable dam bao rang day chi la len ke hoach chua thuc thi ngay 
            // No se them dieu kien truy van where ben duoi 
            var queryVariant = db.Variant_Phone
                                .Include(v => v.Product)
                                .Include(v => v.Product.Phone)
                                .AsQueryable();
            // Neu nhu search co string khac null hay ""
            if (!string.IsNullOrEmpty(searchQuery))
            {
                queryVariant = queryVariant.Where(v => v.Product.product_name.Contains(searchQuery));
            }
            // Neu nhu co loc hang 
            if (companyId.HasValue)
            {
                queryVariant = queryVariant.Where(v => v.Product.company_id == companyId.Value);
            }
            // Neu nhu co min price
            if (minPrice.HasValue)
            {
                queryVariant = queryVariant.Where(v => v.variant_ph_final_price >= minPrice.Value);
            }
            // Neu nhu co max price
            if (maxPrice.HasValue)
            {
                queryVariant = queryVariant.Where(v => v.variant_ph_final_price <= maxPrice.Value);
            }
            // Neu nhu co sort (de sau where - logic sql)
            // Neu nhu co ram
            if (ram.HasValue)
            {
                queryVariant = queryVariant.Where(v => v.variant_ph_ram == ram.Value);
            }
            // Neu nhu co rom
            if (rom.HasValue)
            {
                queryVariant = queryVariant.Where(v => v.variant_ph_ram == rom.Value);
            }
            // Neu nhu co he dieu hanh
            if (!string.IsNullOrEmpty(os))
            {
                queryVariant = queryVariant.Where(v => v.Product.Phone.phone_system.Contains(os));
            }
            //Sap xep
            switch (sort)
            {
                case "price_desc":
                    queryVariant = queryVariant.OrderByDescending(v => v.variant_ph_final_price);
                    break;

                case "price_asc":
                    queryVariant = queryVariant.OrderBy(v => v.variant_ph_final_price);
                    break;
                default:
                    queryVariant = queryVariant.OrderByDescending(v => v.variant_id);
                    break;
            }
            // CHUAN BI DU LIEU TRA VE VIEW 
            var ramList = db.Variant_Phone.Select(v => v.variant_ph_ram).Distinct().OrderBy(x => x).ToList();
            var romList = db.Variant_Phone.Select(v => v.variant_ph_rom).Distinct().OrderBy(x => x).ToList();
            var osList = new List<String> { "IOS", "Android" };
            int pageSize = 9;
            int pageNumber = (page ?? 1);

            var model = new PhonesPageViewModel
            {
                Phones = queryVariant.ToPagedList(pageNumber, pageSize),
                Companies = db.Companies.ToList(),

                RamOptions = ramList,
                RomOptions = romList,
                OsOptions = osList,

                SearchQuery = searchQuery,
                CompanyId = companyId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort,
                Ram = ram,
                Rom = rom,
                Os = os
            };
            return View(model);
        }

        // TRa ve danh sach cac san pham sale 
        public ActionResult Sales(int?page)
        {
            var listSales= db.Sales
                           .Where(s => s.sale_start <= DateTime.Now && s.sale_end >= DateTime.Now)
                           .Include(s => s.Variant_Phone)
                           .Include(s => s.Variant_Phone.Product)
                           .ToList();
            int pageSize = 18;
            int pageNumber = (page ?? 1);
            var model = new SalesPageViewModel
            {
                SalesList = listSales.ToPagedList(pageNumber, pageSize)
            };
            return View(model);
        }

        // Tra ve cac danh sach Blog 
        public ActionResult Blogs(string search, int? page)
        {
            var query = db.Blogs.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.blog_name.Contains(search) || b.blog_author.Contains(search));
            }
            query = query.OrderByDescending(b => b.blog_time);
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var model = new BlogsPageViewModel
            {
                Blogs = query.ToPagedList(pageNumber, pageSize),
                Search = search
            };
            return View(model);
        }
    }
}