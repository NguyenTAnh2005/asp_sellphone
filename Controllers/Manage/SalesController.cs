using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using old_phone.Models;

namespace old_phone.Controllers.Manage
{
    public class SalesController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: Sales
        public ActionResult Index()
        {
            var sales = db.Sales.Include(s => s.Variant_Phone).Include(s => s.Variant_Phone.Product);
            return View(sales.ToList());
        }

        // GET: Sales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            return View(sale);
        }

        // GET: Sales/Create
        public ActionResult Create()
        {
            // Dùng LINQ Join để lấy thông tin từ cả hai bảng: Variant_Phone (v) và Product (p)
            var variantsWithProductNames = db.Variant_Phone
                .Join(
                    db.Products, // Bảng thứ hai để Join
                    v => v.product_id, // Khóa ngoại trong Variant_Phone
                    p => p.product_id, // Khóa chính trong Product
                    (v, p) => new // Tạo một đối tượng ẩn danh (Anonymous Object)
                    {
                        variant_id = v.variant_id,
                        // Tạo chuỗi hiển thị: Ví dụ: "iPhone 13 - Đỏ"
                        DisplayName = p.product_name + " - " + v.variant_ph_color
                    }
                )
                .OrderBy(x => x.DisplayName) // Sắp xếp theo tên sản phẩm cho dễ tìm
                .ToList();

            // Tạo SelectList bằng danh sách mới
            // dataValueField: variant_id
            // dataTextField: DisplayName
            ViewBag.variant_id = new SelectList(variantsWithProductNames, "variant_id", "DisplayName");

            return View();
        }

        // POST: Sales/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "sale_id,sale_name,sale_start,sale_end,variant_id")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                db.Sales.Add(sale);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Phải lặp lại logic tạo SelectList cho trường hợp ModelState.IsValid = false
            var variantsWithProductNames = db.Variant_Phone
                .Join(
                    db.Products,
                    v => v.product_id,
                    p => p.product_id,
                    (v, p) => new
                    {
                        variant_id = v.variant_id,
                        DisplayName = p.product_name + " - " + v.variant_ph_color
                    }
                )
                .OrderBy(x => x.DisplayName)
                .ToList();

            ViewBag.variant_id = new SelectList(variantsWithProductNames, "variant_id", "DisplayName", sale.variant_id);
            return View(sale);
        }

        // GET: Sales/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            ViewBag.variant_id = new SelectList(db.Variant_Phone, "variant_id", "variant_ph_color", sale.variant_id);
            return View(sale);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "sale_id,sale_name,sale_start,sale_end,variant_id")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sale).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.variant_id = new SelectList(db.Variant_Phone, "variant_id", "variant_ph_color", sale.variant_id);
            return View(sale);
        }

        // GET: Sales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sales.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            return View(sale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sale sale = db.Sales.Find(id);
            db.Sales.Remove(sale);
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
