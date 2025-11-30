using old_phone.Common;
using old_phone.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace old_phone.Controllers.Manage
{
    public class ManageProduct_ImageController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManageProduct_Image
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Index(string searchQuery, int?productId, int?page)
        {
            var product_Image = db.Product_Image.Include(pi => pi.Product).AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                product_Image = product_Image.Where(pi => pi.Product.product_name.Contains(searchQuery));
            }
            if (productId.HasValue)
            {
                product_Image = product_Image.Where(pi => pi.product_id == productId.Value);
            }
            product_Image = product_Image.OrderByDescending(pi => pi.image_id);

            var pageSize = 20;
            var pageNumber = (page ?? 1);
            ViewBag.CurrentFilter = searchQuery;
            ViewBag.CurrentProductID = productId;
            ViewBag.ProductList = new SelectList(db.Products, "product_id", "product_name", productId);
            
            var pagedList = product_Image.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // GET: ManageProduct_Image/Details/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product_Image product_Image = db.Product_Image.Find(id);
            if (product_Image == null)
            {
                return HttpNotFound();
            }
            return View(product_Image);
        }

        // GET: ManageProduct_Image/Create
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create()
        {
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name");
            return View();
        }

        // POST: ManageProduct_Image/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create([Bind(Include = "image_id,product_id,image_url")] Product_Image product_Image)
        {
            if (ModelState.IsValid)
            {
                db.Product_Image.Add(product_Image);
                db.SaveChanges();
                TempData["Message"] = "Tạo mới ảnh sản phẩm thành công!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }

            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", product_Image.product_id);
            return View(product_Image);
        }

        // GET: ManageProduct_Image/Edit/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product_Image product_Image = db.Product_Image.Find(id);
            if (product_Image == null)
            {
                return HttpNotFound();
            }
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", product_Image.product_id);
            return View(product_Image);
        }

        // POST: ManageProduct_Image/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit([Bind(Include = "image_id,product_id,image_url")] Product_Image product_Image)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product_Image).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Message"] = "Cập nhật ảnh sản phẩm thành công!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", product_Image.product_id);
            return View(product_Image);
        }

        // GET: ManageProduct_Image/Delete/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product_Image product_Image = db.Product_Image.Find(id);
            if (product_Image == null)
            {
                return HttpNotFound();
            }
            return View(product_Image);
        }

        // POST: ManageProduct_Image/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult DeleteConfirmed(int id)
        {
            Product_Image product_Image = db.Product_Image.Find(id);
            db.Product_Image.Remove(product_Image);
            db.SaveChanges();
            TempData["Message"] = "Xóa ảnh sản phẩm thành công!";
            TempData["MsgType"] = "success";
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
