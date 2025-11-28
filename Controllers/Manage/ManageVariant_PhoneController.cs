using old_phone.Common;
using old_phone.Models;
using old_phone.ViewModels;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace old_phone.Controllers.Manage
{
    public class ManageVariant_PhoneController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManageVariant_Phone
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Index(string searchQuery, int? companyId, int? page)
        {
            var variant_Phone = db.Variant_Phone
                .Include(v => v.Product)
                .Include(v => v.Product.Company) // Include thêm Company để hiện tên hãng
                .Include(v => v.Stock)
                .OrderByDescending(v => v.variant_id)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                variant_Phone = variant_Phone.Where(v => v.Product.product_name.Contains(searchQuery));
            }
            if (companyId.HasValue)
            {
                variant_Phone = variant_Phone.Where(v => v.Product.company_id == companyId.Value);
            }
            int pageSize = 20; 
            int pageNumber = (page ?? 1);
            ViewBag.CurrentFilter = searchQuery;
            ViewBag.CurrentCompanyID = companyId;
            ViewBag.CompanyList = new SelectList(db.Companies, "company_id", "company_name", companyId);

            return View(variant_Phone.ToPagedList(pageNumber, pageSize));
        }

        // GET: ManageVariant_Phone/Details/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Variant_Phone variant_Phone = db.Variant_Phone
                                     .Include(v => v.Product) 
                                     .Include(v => v.Stock)
                                     .FirstOrDefault(v => v.variant_id == id);
            if (variant_Phone == null)
            {
                return HttpNotFound();
            }
            return View(variant_Phone);
        }

        // Create mới dựa trên View Model
        [HttpGet]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create()
        {
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name");
            return View();
        }

        // POST: ManageVariant_Phone/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create(ManageVariant_PhoneViewModel model)
        {
            // A. Validate thủ công (Nếu bạn chưa làm IValidatableObject bên Model)
            if (model.variant_ph_new_price > model.variant_ph_org_price)
            {
                ModelState.AddModelError("variant_ph_new_price", "Giá niêm yết (Mới) không được cao hơn giá gốc.");
            }
            if (ModelState.IsValid)
            {
                Variant_Phone variant_Phone = new Variant_Phone
                {
                    product_id = model.product_id,
                    variant_ph_ram = model.variant_ph_ram,
                    variant_ph_rom = model.variant_ph_rom,
                    variant_ph_color = model.variant_ph_color,
                    variant_ph_org_price = model.variant_ph_org_price,
                    variant_ph_new_price = model.variant_ph_new_price,
                    variant_ph_final_price = model.variant_ph_new_price, // Tính giá final
                    variant_ph_img = model.variant_ph_img,
                    variant_ph_state = model.variant_ph_state
                };
                db.Variant_Phone.Add(variant_Phone);
                db.SaveChanges();
                // Tạo bản ghi Stock tương ứng
                Stock stock = new Stock
                {
                    variant_id = variant_Phone.variant_id,
                    stock_count = model.InitialStockCount
                };
                db.Stocks.Add(stock);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", model.product_id);
            return View(model);
        }

        // GET: ManageVariant_Phone/Edit/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(int? id)
        {
            var variant = db.Variant_Phone
                    .Include(v => v.Product) 
                    .Include(v => v.Stock)
                    .FirstOrDefault(v => v.variant_id == id);
            if (variant == null)
            {
                return HttpNotFound();
            }
            var stock = variant.Stock; 

            var model = new ManageVariant_PhoneViewModel
            {
                variant_id = variant.variant_id,
                product_id = variant.Product.product_id,
                variant_ph_ram = variant.variant_ph_ram,
                variant_ph_rom = variant.variant_ph_rom,
                variant_ph_color = variant.variant_ph_color,
                variant_ph_org_price = variant.variant_ph_org_price,
                variant_ph_new_price = variant.variant_ph_new_price,
                variant_ph_img = variant.variant_ph_img,
                variant_ph_state = variant.variant_ph_state,
                InitialStockCount = stock != null ? stock.stock_count : 0
            };

            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", model.product_id);
            ViewBag.ProductName = variant.Product?.product_name ?? "Sản phẩm không xác định";

            return View(model);
        }

        // POST: ManageVariant_Phone/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(ManageVariant_PhoneViewModel model)
        {
            // A. Validate giá thủ công
            if (model.variant_ph_new_price > model.variant_ph_org_price)
            {
                ModelState.AddModelError("", "Giá niêm yết (Mới) không được cao hơn giá gốc.");
            }

            if (ModelState.IsValid)
            {
                var variantInDb = db.Variant_Phone
                                    .Include(v => v.Stock)
                                    .FirstOrDefault(v => v.variant_id == model.variant_id);

                if (variantInDb != null)
                {
                    variantInDb.product_id = model.product_id;
                    variantInDb.variant_ph_ram = model.variant_ph_ram;
                    variantInDb.variant_ph_rom = model.variant_ph_rom;
                    variantInDb.variant_ph_color = model.variant_ph_color;
                    variantInDb.variant_ph_org_price = model.variant_ph_org_price;
                    variantInDb.variant_ph_new_price = model.variant_ph_new_price;
                    variantInDb.variant_ph_final_price = model.variant_ph_new_price;
                    variantInDb.variant_ph_img = model.variant_ph_img;
                    variantInDb.variant_ph_state = model.variant_ph_state;
                    if (variantInDb.Stock == null)
                    {
                        variantInDb.Stock = new Stock
                        {
                            variant_id = variantInDb.variant_id,
                            stock_count = model.InitialStockCount
                        };
                        db.Stocks.Add(variantInDb.Stock);
                    }
                    else
                    {
                        variantInDb.Stock.stock_count = model.InitialStockCount;
                    }

                    // 4. Lưu thay đổi
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy biến thể cần sửa.");
                }
            }

            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", model.product_id);

            // Lấy lại tên sản phẩm để hiển thị trên tiêu đề (tránh null)
            var prodName = db.Products.Find(model.product_id)?.product_name;
            ViewBag.ProductName = prodName;

            return View(model);
        }
        // GET: ManageVariant_Phone/Delete/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Variant_Phone variant_Phone = db.Variant_Phone.Find(id);
            if (variant_Phone == null)
            {
                return HttpNotFound();
            }
            return View(variant_Phone);
        }

        // POST: ManageVariant_Phone/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult DeleteConfirmed(int id)
        {
            Variant_Phone variant_Phone = db.Variant_Phone.Find(id);
            Stock stock = db.Stocks.FirstOrDefault(s => s.variant_id == variant_Phone.variant_id);
            if (stock != null)
            {
                db.Stocks.Remove(stock);
            }
            db.Variant_Phone.Remove(variant_Phone);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
