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
                    form_data.account_password = HashPassword.Encrypt(form_data.account_password);
                    form_data.role_id = 1;
                    form_data.account_date = DateTime.Now;

                    db.Accounts.Add(form_data);
                    db.SaveChanges();

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
            return View();
        }
        // Dang nhap gui Post de check va tra ve view tuong ung 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Account form_data)
        {
            if (ModelState.IsValid)
            {
                form_data.account_password = HashPassword.Encrypt(form_data.account_password);
                var account = db.Accounts.FirstOrDefault(a => a.account_email == form_data.account_email && a.account_password == form_data.account_password);

                if (account != null)
                {
                    // Tao session luu TT tai khoan 
                    Session["account"] = account;
                    Session["acc_id"] = account.account_id;
                    Session["acc_name"] = account.account_last_name + " " + account.account_first_name;
                    Session["acc_role"] = account.role_id;

                    return RedirectToAction("Index", "Shop");
                }
                else
                {
                    ViewBag.Error="Tên đăng nhập hoặc mật khẩu không đúng";
                    return View();
                }
            }
            return View();
        }

        // Dang xuat 
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Shop");
        }
    }
}