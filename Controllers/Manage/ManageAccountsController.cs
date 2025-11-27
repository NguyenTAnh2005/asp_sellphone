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
    public class ManageAccountsController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // GET: ManageAccounts
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Index(string acc_email, string acc_name, int? role_id, int?page )
        {
            var accounts = db.Accounts.Include(a => a.Role).AsQueryable();
            if (!string.IsNullOrEmpty(acc_email))
            {
                accounts = accounts.Where(a => a.account_email.Contains(acc_email));
            }
            if (!string.IsNullOrEmpty(acc_name))
            {
                accounts = accounts.Where(a => (a.account_first_name + " " + a.account_last_name).Contains(acc_name));
            }
            if (role_id.HasValue)
            {
                accounts = accounts.Where(a => a.role_id == role_id.Value);
            }
            accounts = accounts.OrderByDescending(a => a.account_id);
            if (accounts == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản nào!";
            }
            var roles = db.Roles.ToList();
            var roleSelectList = new SelectList(roles, "role_id", "role_name", role_id);
            
            ViewBag.RoleList = roleSelectList;
            ViewBag.CurrentEmailFilter = acc_email;
            ViewBag.CurrentNameFilter = acc_name;
            var pageSize = 20;
            var pageNumber = (page ?? 1);
            var PageList = accounts.ToPagedList(pageNumber, pageSize);

            return View(PageList);
        }

        // GET: ManageAccounts/Details/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // GET: ManageAccounts/Create
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create()
        {
            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name");
            return View();
        }

        // POST: ManageAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create([Bind(Include = "account_id,account_first_name,account_last_name,account_email,account_gender,account_date,account_password,role_id,RememberMeToken,TokenExpiryDate")] Account account)
        {
            account.account_password = Utility.Encrypt(account.account_password);
            if (ModelState.IsValid)
            {
                var accountExists = db.Accounts.FirstOrDefault(a => a.account_email == account.account_email);
                if (accountExists == null)
                {
                    db.Accounts.Add(account);
                    db.SaveChanges();
                    ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
                    return RedirectToAction("Index");
                }
                ViewBag.Error = "Email đã được sử dụng bởi tài khoản khác!";
                ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
                return View();
            }

            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
            return View(account);
        }

        // GET: ManageAccounts/Edit/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
            return View(account);
        }

        // POST: ManageAccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Edit([Bind(Include = "account_id,account_first_name,account_last_name,account_email,account_gender,account_date,account_password,role_id,RememberMeToken,TokenExpiryDate")] Account account)
        {
            ModelState.Remove("account_password");
            // Ngăn chặn ko có 1 admin (chuyển quyền user cho admin duy nhất hiện tại )
            if (account.role_id != 2)
            {
                //thêm.AsNoTracking() vào câu lệnh truy vấn listAdmin. Điều này bảo EF: "Lấy dữ liệu ra để tôi kiểm tra thôi, đừng theo dõi hay lưu nó vào bộ nhớ làm gì".
                var listAdmin = db.Accounts.AsNoTracking().Where(a => a.role_id == 2).ToList();
                if (listAdmin.Count <= 1 && listAdmin[0].account_id == account.account_id)
                {
                    ViewBag.Error = "Không thể chuyển quyền của tài khoản quản trị cuối cùng!";
                    ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
                    return View(account);
                }
            }
            if (ModelState.IsValid)
            {
                // Lấy tài khoản cũ từ DB
                var oldAccount = db.Accounts.AsNoTracking().FirstOrDefault(a => a.account_id == account.account_id);

                //  Gán lại mật khẩu cũ cho đối tượng mới
                if (oldAccount != null)
                {
                    account.account_password = oldAccount.account_password;
                    account.RememberMeToken = oldAccount.RememberMeToken;
                    account.TokenExpiryDate = oldAccount.TokenExpiryDate;
                }
                
                //  Lưu lại
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);

            return View(account);
        }

        // GET: ManageAccounts/Delete/5
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: ManageAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Accounts.Find(id);
            if (account.role_id == 2)
            {
                var listAdmin = db.Accounts.Where(a => a.role_id == 2).ToList();
                if (listAdmin.Count <= 1 && listAdmin[0].account_id == account.account_id)
                {
                    ViewBag.Error = "Không thể xóa tài khoản quản trị cuối cùng!";
                    return View(account);
                }
            }
            db.Accounts.Remove(account);
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
