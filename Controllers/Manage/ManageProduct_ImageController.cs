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
    public class ManageProduct_ImageController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManageProduct_Image
        public ActionResult Index()
        {
            var product_Image = db.Product_Image.Include(p => p.Product);
            return View(product_Image.ToList());
        }

        // GET: ManageProduct_Image/Details/5
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
        public ActionResult Create()
        {
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name");
            return View();
        }

        // POST: ManageProduct_Image/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "image_id,product_id,image_url")] Product_Image product_Image)
        {
            if (ModelState.IsValid)
            {
                db.Product_Image.Add(product_Image);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", product_Image.product_id);
            return View(product_Image);
        }

        // GET: ManageProduct_Image/Edit/5
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
        public ActionResult Edit([Bind(Include = "image_id,product_id,image_url")] Product_Image product_Image)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product_Image).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", product_Image.product_id);
            return View(product_Image);
        }

        // GET: ManageProduct_Image/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            Product_Image product_Image = db.Product_Image.Find(id);
            db.Product_Image.Remove(product_Image);
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
