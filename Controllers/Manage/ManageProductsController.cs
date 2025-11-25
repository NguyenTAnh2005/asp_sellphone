using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using old_phone.Models;
using old_phone.Common;

namespace old_phone.Controllers.Manage
{
    public class ManageProductsController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManageProducts
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Company).Include(p => p.Phone);
            return View(products.ToList());
        }

        // GET: ManageProducts/Details/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: ManageProducts/Create
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create()
        {
            ViewBag.company_id = new SelectList(db.Companies, "company_id", "company_name");
            ViewBag.product_id = new SelectList(db.Phones, "product_id", "phone_chip");
            return View();
        }

        // POST: ManageProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create([Bind(Include = "product_id,company_id,product_name,product_desc")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.company_id = new SelectList(db.Companies, "company_id", "company_name", product.company_id);
            ViewBag.product_id = new SelectList(db.Phones, "product_id", "phone_chip", product.product_id);
            return View(product);
        }

        // GET: ManageProducts/Edit/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.company_id = new SelectList(db.Companies, "company_id", "company_name", product.company_id);
            ViewBag.product_id = new SelectList(db.Phones, "product_id", "phone_chip", product.product_id);
            return View(product);
        }

        // POST: ManageProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit([Bind(Include = "product_id,company_id,product_name,product_desc")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.company_id = new SelectList(db.Companies, "company_id", "company_name", product.company_id);
            ViewBag.product_id = new SelectList(db.Phones, "product_id", "phone_chip", product.product_id);
            return View(product);
        }

        // GET: ManageProducts/Delete/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: ManageProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            try
            {
                db.Products.Remove(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                return View("~/Views/Shared/Error_Delete.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("~/Views/Shared/Error_Delete.cshtml");
            }

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
