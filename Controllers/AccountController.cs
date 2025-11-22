using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using old_phone.Common;
using old_phone.Models;

namespace old_phone.Controllers
{
    public class AccountController : Controller
    {
        private OldPhoneEntities db = new OldPhoneEntities();

        // DANG KY: Hien thi form 

        [HttpGet]
        public ActionResult Signup()
        {
            if (Session["account"] != null)
            {
                return RedirectToAction("Index", "Shop");
            }
            return View();
        }

        //DANG KY:  POST form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(Account form_data)
        {
            // Kiem tra email co ton tai chua
            // Chua co thi tao moi, luu vao CSDL va chuyen ve trang LogIn
            if (ModelState.IsValid)
            {
                var check = db.Accounts.FirstOrDefault(a => a.account_email == form_data.account_email);
                if (check == null)
                {
                    form_data.account_password = Utility.Encrypt(form_data.account_password);
                    form_data.role_id = 1;
                    form_data.account_date = DateTime.Now;

                    db.Accounts.Add(form_data);
                    db.SaveChanges();
                    TempData["Message"] = "Đăng ký thành công!";
                    TempData["MsgType"] = "success"; // success, danger, warning, info
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "Email này đã được đăng ký. Vui lòng chọn email khác.";
                    return View();
                }
            }

            return View();
        }

        // Dang nhap 
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["account"] != null)
            {
                return RedirectToAction("Index", "Shop");
            }
            return View();
        }
        // Dang nhap gui Post de check va tra ve view tuong ung 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string account_email, string account_password, bool rememberMe=false)
        {

            var hash_password = Utility.Encrypt(account_password);
            var account = db.Accounts.FirstOrDefault(a => a.account_email == account_email && a.account_password == hash_password);

            if (account != null)
            {
                // Tao session luu TT tai khoan 
                Session["account"] = account;
                Session["acc_id"] = account.account_id;
                Session["acc_name"] = account.account_last_name + " " + account.account_first_name;
                Session["acc_role"] = account.role_id;
                if (rememberMe)
                {
                    string token = Utility.GenerateNewToken();
                    DateTime expiryToken = DateTime.Now.AddDays(30);

                    account.RememberMeToken = token;
                    account.TokenExpiryDate = expiryToken;
                    db.Entry(account).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    HttpCookie cookie = new HttpCookie("KeepLogin");
                    cookie.Values["token"] = token;
                    cookie.Expires = expiryToken;
                    cookie.HttpOnly = true; // CHong tan cong XSS

                    //Gửi đối tượng Cookie mà Server vừa tạo ra xuống
                    //máy tính của người dùng thông qua Header của phản hồi HTTP
                    Response.Cookies.Add(cookie);
                }
                else
                {
                    // Nếu người dùng không chọn ghi nhớ, hủy Token cũ nếu có
                    if (!string.IsNullOrEmpty(account.RememberMeToken))
                    {
                        account.RememberMeToken = null;
                        account.TokenExpiryDate = null;
                        db.Entry(account).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                // Gán thông báo trực tiếp
                TempData["Message"] = "Đăng nhập thành công!";
                TempData["MsgType"] = "success"; // success, danger, warning, info
                return RedirectToAction("Index", "Shop");
            }
            else
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }
        }

        // Dang xuat 
        public ActionResult Logout()
        {
            // Xoa Session, xoa token trong DB, xoa Cookie 

            int? id_acc_loggedIn = Session["acc_id"] as int?;

            Session.Clear();
            if (id_acc_loggedIn.HasValue)
            {
                var accountToUpdate = db.Accounts.Find(id_acc_loggedIn);
                if (accountToUpdate != null)
                {
                    accountToUpdate.RememberMeToken = null;
                    accountToUpdate.TokenExpiryDate = null;
                    db.Entry(accountToUpdate).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            
            if (Request.Cookies["KeepLogin"] != null)
            {
                var cookie = new HttpCookie("KeepLogin");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            TempData["Message"] = "Đăng xuất thành công!";
            TempData["MsgType"] = "success"; // success, danger, warning, info
            return RedirectToAction("Index", "Shop");
        }
    }
}