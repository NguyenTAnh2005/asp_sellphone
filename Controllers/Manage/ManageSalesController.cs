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
                    DisplayText = $"{v.Product.product_name} - {v.variant_ph_color} ({v.variant_ph_ram}GB/{v.variant_ph_rom}GB) - Giá niêm yết ban đầu: {v.variant_ph_new_price:N0}đ"
                });
            ViewBag.variant_id = new SelectList(variants, "variant_id", "DisplayText", selectedVariant);
        }

        // GET  INDEX 
        [AuthorizeCheck(RequiredRole =2)]
        public ActionResult Index(string eventName, int? page)
        {
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
                var variant = db.Variant_Phone.Find(model.variant_id);
                if (variant != null)
                {
                    variant.variant_ph_final_price = model.DiscountPrice; // Cập nhật giá giảm
                    db.Entry(variant).State = EntityState.Modified;
                }

                db.SaveChanges();
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
            if (ModelState.IsValid)
            {
                // A. Cập nhật bảng Sale
                var sale = db.Sales.Find(model.sale_id);
                sale.sale_name = model.sale_name;
                sale.sale_start = model.sale_start;
                sale.sale_end = model.sale_end;

                // B. Cập nhật lại giá Final trong bảng Variant
                var variant = db.Variant_Phone.Find(model.variant_id);
                if (variant != null)
                {
                    variant.variant_ph_final_price = model.DiscountPrice; // Cập nhật giá giảm mới
                    db.Entry(variant).State = EntityState.Modified;
                }

                db.SaveChanges();
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
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

       
    }
}