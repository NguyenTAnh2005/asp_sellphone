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
using System.Web.UI;

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeCheck(RequiredRole = 2)]
        public ActionResult Create([Bind(Include = "account_id,account_first_name,account_last_name,account_email,account_gender,account_date,account_password,role_id,RememberMeToken,TokenExpiryDate")] Account account)
        {
            account.account_password = Utility.Encrypt(account.account_password);
            var today = DateTime.Today;
            var age = today.Year - account.account_date.Year;
            // Nếu chưa tới sinh nhật năm nay thì trừ 1 tuổi
            if (account.account_date.Date > today.AddYears(-age)) age--;

            if (account.account_date.Date > today)
            {
                ViewBag.Error = "Ngày sinh không được lớn hơn ngày hiện tại.";
                ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
                return View();
            }
            if (age < 15)
            {
                ViewBag.Error = "Độ tuổi đăng ký tài khoản phải từ 15 tuổi";
                ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
                return View();
            }
            if (age > 100)
            {
                ViewBag.Error = "Độ tuổi đăng ký tài khoản quá lớn (>100)";
                ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
                return View();
            }

            if (ModelState.IsValid)
            {
                var accountExists = db.Accounts.FirstOrDefault(a => a.account_email == account.account_email);
                if (accountExists == null)
                {
                    db.Accounts.Add(account);
                    db.SaveChanges();
                    ViewBag.role_id = new SelectList(db.Roles, "role_id", "role_name", account.role_id);
                    TempData["Message"] = "Tạo mới tài khoản thành công!";
                    TempData["MsgType"] = "success"; // success, danger, warning, info
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
                TempData["Message"] = "Cập nhật tài khoản thành công!";
                TempData["MsgType"] = "success"; // success, danger, warning, info
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
            // Không xóa hẳn khỏi DB, chỉ đổi thông tin để tránh mất dữ liệu liên quan
            account.account_first_name += " Người dùng đã bị xóa";

            // Quan trọng: Email thường có Unique Constraint, nên phải thêm ID vào đuôi
            account.account_email = "deleted_" + account.account_id + "_" + Guid.NewGuid().ToString().Substring(0, 5) + "@system.local";

            // Đổi mật khẩu thành chuỗi không thể giải mã/đoán được
            account.account_password = Guid.NewGuid().ToString();
            db.SaveChanges();
            TempData["Message"] = "(Xóa) tài khoản thành công!";
            TempData["MsgType"] = "success"; // success, danger, warning, info
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
