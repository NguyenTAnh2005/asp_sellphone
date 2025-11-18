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
    public class ManageVariant_PhoneController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManageVariant_Phone
        public ActionResult Index()
        {
            var variant_Phone = db.Variant_Phone.Include(v => v.Product).Include(v => v.Stock);
            return View(variant_Phone.ToList());
        }

        // GET: ManageVariant_Phone/Details/5
        public ActionResult Details(int? id)
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

        // GET: ManageVariant_Phone/Create
        public ActionResult Create()
        {
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name");
            ViewBag.variant_id = new SelectList(db.Stocks, "variant_id", "variant_id");
            return View();
        }

        // POST: ManageVariant_Phone/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "variant_id,product_id,variant_ph_ram,variant_ph_rom,variant_ph_color,variant_ph_org_price,variant_ph_new_price,variant_ph_final_price,variant_ph_img")] Variant_Phone variant_Phone)
        {
            if (ModelState.IsValid)
            {
                db.Variant_Phone.Add(variant_Phone);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", variant_Phone.product_id);
            ViewBag.variant_id = new SelectList(db.Stocks, "variant_id", "variant_id", variant_Phone.variant_id);
            return View(variant_Phone);
        }

        // GET: ManageVariant_Phone/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", variant_Phone.product_id);
            ViewBag.variant_id = new SelectList(db.Stocks, "variant_id", "variant_id", variant_Phone.variant_id);
            return View(variant_Phone);
        }

        // POST: ManageVariant_Phone/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "variant_id,product_id,variant_ph_ram,variant_ph_rom,variant_ph_color,variant_ph_org_price,variant_ph_new_price,variant_ph_final_price,variant_ph_img")] Variant_Phone variant_Phone)
        {
            if (ModelState.IsValid)
            {
                db.Entry(variant_Phone).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", variant_Phone.product_id);
            ViewBag.variant_id = new SelectList(db.Stocks, "variant_id", "variant_id", variant_Phone.variant_id);
            return View(variant_Phone);
        }

        // GET: ManageVariant_Phone/Delete/5
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
        public ActionResult DeleteConfirmed(int id)
        {
            Variant_Phone variant_Phone = db.Variant_Phone.Find(id);
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
