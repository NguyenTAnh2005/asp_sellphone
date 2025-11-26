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
using PagedList;

namespace old_phone.Controllers.Manage
{
    public class ManagePhonesController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManagePhones
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Index(string searchQuery, int? companyId, int? page)
        {
            var phones = db.Phones
                .Include(p => p.Product)
                .Include(p => p.Product.Company)
                .OrderByDescending(p => p.product_id)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                phones = phones.Where(p => p.Product.product_name.Contains(searchQuery));
            }
            if (companyId.HasValue)
            {
                phones = phones.Where(p => p.Product.company_id == companyId.Value);
            }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            ViewBag.CurrentFilter = searchQuery;
            ViewBag.CurrentCompanyID = companyId;
            ViewBag.CompanyList = new SelectList(db.Companies, "company_id", "company_name",companyId);

            var pagedPhones = phones.ToPagedList(pageNumber, pageSize);
            return View(pagedPhones);
        }

        // GET: ManagePhones/Details/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Phone phone = db.Phones.Find(id);
            if (phone == null)
            {
                return HttpNotFound();
            }
            return View(phone);
        }

        // GET: ManagePhones/Create
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create()
        {
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name");
            return View();
        }

        // POST: ManagePhones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create([Bind(Include = "product_id,phone_chip,phone_screen_size,phone_front_cam,phone_behind_cam,phone_battery,phone_system,phone_charging_port,phone_sim_card,phone_nfc,phone_ear_phone,phone_memory_card")] Phone phone)
        {
            if (ModelState.IsValid)
            {
                db.Phones.Add(phone);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", phone.product_id);
            return View(phone);
        }

        // GET: ManagePhones/Edit/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Phone phone = db.Phones.Find(id);
            if (phone == null)
            {
                return HttpNotFound();
            }
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", phone.product_id);
            return View(phone);
        }

        // POST: ManagePhones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit([Bind(Include = "product_id,phone_chip,phone_screen_size,phone_front_cam,phone_behind_cam,phone_battery,phone_system,phone_charging_port,phone_sim_card,phone_nfc,phone_ear_phone,phone_memory_card")] Phone phone)
        {
            if (ModelState.IsValid)
            {
                db.Entry(phone).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.product_id = new SelectList(db.Products, "product_id", "product_name", phone.product_id);
            return View(phone);
        }

        // GET: ManagePhones/Delete/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Phone phone = db.Phones.Find(id);
            if (phone == null)
            {
                return HttpNotFound();
            }
            return View(phone);
        }

        // POST: ManagePhones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult DeleteConfirmed(int id)
        {
            Phone phone = db.Phones.Find(id);
            var list_variant = db.Variant_Phone.Where(v => v.product_id == id).ToList();
            try
            {
                if (list_variant.Count() == 0)
                {
                    db.Phones.Remove(phone);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("~/Views/Shared/Error_Delete.cshtml");
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                return View("~/Views/Shared/Error_Delete.cshtml");
            }
            catch (Exception ex) 
            {
                ViewBag.ErrorMessage= ex.Message;
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
