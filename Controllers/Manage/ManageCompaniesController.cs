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
    public class ManageCompaniesController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManageCompanies
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Index()
        {
            return View(db.Companies.ToList());
        }

        // GET: ManageCompanies/Details/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // GET: ManageCompanies/Create
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ManageCompanies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create([Bind(Include = "company_id,company_name,company_desc")] Company company)
        {
            if (ModelState.IsValid)
            {
                db.Companies.Add(company);
                db.SaveChanges();
                TempData["Message"] = "Tạo mới nhà cung cấp thành công!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }

            return View(company);
        }

        // GET: ManageCompanies/Edit/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: ManageCompanies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit([Bind(Include = "company_id,company_name,company_desc")] Company company)
        {
            if (ModelState.IsValid)
            {
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Message"] = "Chỉnh sửa nhà cung cấp thành công!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }
            return View(company);
        }

        // GET: ManageCompanies/Delete/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: ManageCompanies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult DeleteConfirmed(int id)
        {
            Company company = db.Companies.Find(id);
            try
            {
                db.Companies.Remove(company);
                db.SaveChanges();
                TempData["Message"] = "Xóa nhà cung cấp thành công!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                return View("~/Views/Shared/Error_Delete.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("~/Views/Shared/Error.cshtml");
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
