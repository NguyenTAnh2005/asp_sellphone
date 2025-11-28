using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using old_phone.Models;
using old_phone.ViewModels;
using PagedList;
using old_phone.Common;

namespace old_phone.Controllers.Manage
{
    public class ManageSalesController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // HÀM HỖ TRỢ DROPDOWN
        private void PopulateVariantDropDownList(object selectedVariant = null)
        {
            var variants = db.Variant_Phone.Include(v => v.Product).ToList()
                .Select(v => new {
                    variant_id = v.variant_id,
                    DisplayText = $"{v.Product.product_name} - {v.variant_ph_color} ({v.variant_ph_ram}GB/{v.variant_ph_rom}GB) - {v.variant_ph_color}"
                });
            ViewBag.variant_id = new SelectList(variants, "variant_id", "DisplayText", selectedVariant);
        }

        // HÀM TỰ ĐỘNG XÓA SALE HẾT HẠN
        private void AutoRemoveExpiredSales()
        {
            var expiredSales = db.Sales
                .Where(s => s.sale_end < DateTime.Now)
                .Include(s => s.Variant_Phone)
                .ToList();

            if (expiredSales.Any())
            {
                foreach (var expiredSale in expiredSales)
                {
                    // Reset giá Final về giá New (hết khuyến mãi)
                    if (expiredSale.Variant_Phone != null)
                    {
                        expiredSale.Variant_Phone.variant_ph_final_price = expiredSale.Variant_Phone.variant_ph_new_price;
                        db.Entry(expiredSale.Variant_Phone).State = EntityState.Modified;
                    }

                    // Xóa Sale đã hết hạn
                    db.Sales.Remove(expiredSale);
                }

                db.SaveChanges();

                // Thông báo cho admin biết đã tự động xóa
                TempData["Message"] = $"Đã tự động xóa {expiredSales.Count} chương trình khuyến mãi hết hạn và reset giá sản phẩm.";
                TempData["MsgType"] = "info";
            }
        }

        // GET  INDEX 
        [AuthorizeCheck(RequiredRole =2)]
        public ActionResult Index(string eventName, int? page)
        {
            // Tự động xóa các sale hết hạn
            AutoRemoveExpiredSales();

            var sales = db.Sales.Include(s => s.Variant_Phone).Include(s => s.Variant_Phone.Product).AsQueryable();
            var saleNames = db.Sales.Select(s => s.sale_name).Distinct().OrderBy(n => n).ToList();
            ViewBag.eventList = new SelectList(saleNames, eventName);

            if (!String.IsNullOrEmpty(eventName))
            {
                sales = sales.Where(s => s.sale_name == eventName);
            }
            sales = sales.OrderByDescending(s => s.sale_start);
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.CurrentFilter = eventName;
            return View(sales.ToPagedList(pageNumber, pageSize));
        }

        //GET DETAILS
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Sale sale = db.Sales.Find(id);
            if (sale == null) return HttpNotFound();
            return View(sale);
        }

        // GET CREATE
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create()
        {
            PopulateVariantDropDownList();
            return View();
        }

        // 3. CREATE (POST) 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create(ManageSaleViewModel model)
        {
            // Lấy giá hiện tại từ DB để validate
            var variant = db.Variant_Phone.Find(model.variant_id);
            if (variant != null)
            {
                model.CurrentNewPrice = variant.variant_ph_new_price; // Gán giá để validate

                if (model.DiscountPrice >= model.CurrentNewPrice)
                {
                    ModelState.AddModelError("DiscountPrice", "Giá khuyến mãi phải nhỏ hơn giá niêm yết hiện tại.");
                }
            }
            else
            {
                ModelState.AddModelError("variant_id", "Sản phẩm không tồn tại.");
            }

            if (model.sale_end < DateTime.Now)
            {
                ModelState.AddModelError("sale_end", "Ngày kết thúc khuyến mãi phải sau ngày hiện tại.");
            }
            if (model.sale_end <= model.sale_start)
            {
                ModelState.AddModelError("sale_end", "Ngày kết thúc khuyến mãi phải lớn hơn ngày bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                // A. Lưu bảng Sale
                Sale sale = new Sale
                {
                    sale_name = model.sale_name,
                    sale_start = model.sale_start,
                    sale_end = model.sale_end,
                    variant_id = model.variant_id
                };
                db.Sales.Add(sale);

                // B. Cập nhật giá Final trong bảng Variant
                variant.variant_ph_final_price = model.DiscountPrice; // Cập nhật giá giảm
                db.Entry(variant).State = EntityState.Modified;

                db.SaveChanges();
                TempData["Message"] = "Đã tạo chương trình khuyến mãi thành công!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }

            PopulateVariantDropDownList(model.variant_id);
            return View(model);
        }

        // 4. EDIT (GET)
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Sale sale = db.Sales.Find(id);
            if (sale == null) return HttpNotFound();

            // Map dữ liệu sang ViewModel để hiển thị
            var model = new ManageSaleViewModel
            {
                sale_id = sale.sale_id,
                sale_name = sale.sale_name,
                sale_start = sale.sale_start,
                sale_end = sale.sale_end,
                variant_id = sale.variant_id ?? 0,

                // Lấy giá hiện tại để hiển thị
                CurrentNewPrice = sale.Variant_Phone.variant_ph_new_price,
                DiscountPrice = sale.Variant_Phone.variant_ph_final_price
            };

            PopulateVariantDropDownList(sale.variant_id);
            return View(model);
        }

        // 4. EDIT (POST) - LOGIC CẬP NHẬT GIÁ
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(ManageSaleViewModel model)
        {
            // Lấy giá hiện tại từ DB để validate
            var variant = db.Variant_Phone.Find(model.variant_id);
            if (variant != null)
            {
                model.CurrentNewPrice = variant.variant_ph_new_price;

                if (model.DiscountPrice >= model.CurrentNewPrice)
                {
                    ModelState.AddModelError("DiscountPrice", "Giá khuyến mãi phải nhỏ hơn giá niêm yết hiện tại.");
                }
            }

            if (model.sale_end < DateTime.Now)
            {
                ModelState.AddModelError("sale_end", "Ngày kết thúc khuyến mãi phải sau ngày hiện tại.");
            }
            if (model.sale_end <= model.sale_start)
            {
                ModelState.AddModelError("sale_end", "Ngày kết thúc khuyến mãi phải lớn hơn ngày bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                // A. Cập nhật bảng Sale
                var sale = db.Sales.Find(model.sale_id);
                sale.sale_name = model.sale_name;
                sale.sale_start = model.sale_start;
                sale.sale_end = model.sale_end;

                // B. Cập nhật lại giá Final trong bảng Variant
                if (variant != null)
                {
                    variant.variant_ph_final_price = model.DiscountPrice; // Cập nhật giá giảm mới
                    db.Entry(variant).State = EntityState.Modified;
                }

                db.SaveChanges();
                TempData["Message"] = "Đã cập nhật chương trình khuyến mãi thành công!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }
            PopulateVariantDropDownList(model.variant_id);
            return View(model);
        }

        // 5. DELETE (GET)
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Sale sale = db.Sales.Find(id);
            if (sale == null) return HttpNotFound();
            return View(sale);
        }

        // 5. DELETE (POST) - LOGIC TRẢ VỀ GIÁ CŨ
        [AuthorizeCheck(RequiredRole = 2)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sale sale = db.Sales.Find(id);

            // A. Trả giá Final về bằng giá New (Hết Sale)
            var variant = db.Variant_Phone.Find(sale.variant_id);
            if (variant != null)
            {
                variant.variant_ph_final_price = variant.variant_ph_new_price; // Reset giá
                db.Entry(variant).State = EntityState.Modified;
            }

            // B. Xóa Sale
            db.Sales.Remove(sale);
            db.SaveChanges();
            
            TempData["Message"] = "Đã xóa chương trình khuyến mãi và reset giá sản phẩm!";
            TempData["MsgType"] = "success";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // API trả về giá của Variant để JS tự động điền
        [HttpGet]
        public JsonResult GetVariantPrice(int variant_id)
        {
            var variant = db.Variant_Phone.Find(variant_id);
            if (variant != null)
            {
                return Json(new
                {
                    success = true,
                    new_price = variant.variant_ph_new_price,
                    final_price = variant.variant_ph_final_price
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}