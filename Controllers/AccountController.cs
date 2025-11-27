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
            if (Request.Cookies["KeepLogin"] != null)
            {
                // Lays token de kiem tra trong db
                var token = Request.Cookies["KeepLogin"].Values["token"];
                var account = db.Accounts.FirstOrDefault(a => a.RememberMeToken == token);
                // Neu nhu co token do trong DB va con han su dung
                if (account != null && account.TokenExpiryDate > DateTime.Now)
                {
                    // Phuc hoi session
                    Session["account"] = account;
                    Session["acc_id"] = account.account_id;
                    Session["acc_name"] = account.account_last_name + " " + account.account_first_name;
                    Session["acc_role"] = account.role_id;
                }
                else
                {
                    // Phong TH khi dang nhap may tinh, sau do dang nhap them va ghi nho dang nhap o thiet bi khac
                    // se tao ra token moi => ghi de vao database, thi o tren trinh duyet may tinh cookie luc nay co
                    // gia tri token khac voi token trong DB, nen neu nhu truy cap lai tren may tinh, sever se kiem tra token de cho dang nhap
                    // nhung luc nay token tren may tinh da khac so voi trong db do do  cookie tren may tinh se bi bo
                    // bang cach tao 1 cookie trung ten nhung set han expried la ngay hom qua => trinh duyet check => xoa Cookie
                    var expriedCookie = new HttpCookie("KeepLogin");
                    expriedCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(expriedCookie);
                }
            }
            if (Session["account"] != null)
            {
                return RedirectToAction("Index", "Shop");
            }
            return View();
        }

        //ACTION ĐĂNG KÝ (SIGNUP POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(Account form_data)
        {
            if (ModelState.IsValid)
            {
                // --- LOGIC MỚI: KIỂM TRA TUỔI ---
                var today = DateTime.Today;
                var age = today.Year - form_data.account_date.Year;
                // Nếu chưa tới sinh nhật năm nay thì trừ 1 tuổi
                if (form_data.account_date.Date > today.AddYears(-age)) age--;

                if (age < 15)
                {
                    ViewBag.Error = "Bạn phải đủ 15 tuổi mới được đăng ký tài khoản.";
                    return View(form_data); // Trả lại form giữ nguyên dữ liệu đã nhập
                }
                // --------------------------------

                var check = db.Accounts.FirstOrDefault(a => a.account_email == form_data.account_email);
                if (check == null)
                {
                    form_data.account_password = Utility.Encrypt(form_data.account_password);
                    form_data.role_id = 1;

                    // XÓA DÒNG NÀY: form_data.account_date = DateTime.Now; 
                    // (Vì user đã nhập ngày sinh vào account_date rồi)

                    db.Accounts.Add(form_data);
                    db.SaveChanges();
                    TempData["Message"] = "Đăng ký thành công!";
                    TempData["MsgType"] = "success";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.Error = "Email này đã được đăng ký. Vui lòng chọn email khác.";
                    return View(form_data);
                }
            }
            return View(form_data);
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
        public ActionResult Login(string account_email, string account_password, bool rememberMe = false)
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

        // GET: Hiển thị trang nhập Email
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            // Nếu đang đăng nhập thì đá về trang chủ, không cho reset
            if (Session["acc_id"] != null)
            {
                return RedirectToAction("Index", "Shop");
            }
            return View();
        }

        // POST: Xử lý cấp lại mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            // 1. Kiểm tra Email có tồn tại không
            var account = db.Accounts.FirstOrDefault(a => a.account_email == email);

            if (account == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View(); // Trả về View để nhập lại
            }

            // 2. Tạo mật khẩu mới ngẫu nhiên (8 ký tự)
            // Guid.NewGuid() tạo chuỗi dài, ta cắt lấy 8 ký tự đầu làm mật khẩu
            string newPassRaw = Guid.NewGuid().ToString().Substring(0, 8);

            try
            {
                // 3. Gửi Email trước (để chắc chắn gửi được mới lưu DB)
                string subject = "[SellPhone TA] Cấp lại mật khẩu mới";
                string body = $"Chào {account.account_last_name} {account.account_first_name},<br/><br/>" +
                              $"Yêu cầu cấp lại mật khẩu của bạn đã được xử lý.<br/>" +
                              $"Mật khẩu mới của bạn là: <b style='color:red; font-size:18px;'>{newPassRaw}</b><br/><br/>" +
                              $"Vui lòng đăng nhập và đổi lại mật khẩu ngay để bảo mật thông tin.";

                MailHelper.SendEmail(account.account_email, subject, body);

                // 4. Gửi thành công -> Mới lưu vào Database
                // Nhớ dùng hàm Encrypt của bạn để mã hóa trước khi lưu
                account.account_password = old_phone.Common.Utility.Encrypt(newPassRaw);
                db.SaveChanges();

                // 5. Thông báo và chuyển về Login
                TempData["Message"] = "Mật khẩu mới đã được gửi vào Email của bạn. Vui lòng kiểm tra hộp thư!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi gửi email: " + ex.Message;
                return View();
            }
        }

        // Action hiển thị trang Profile
        [AuthorizeCheck]
        public ActionResult AccountProfile()
        {
            var acc_id = Session["acc_id"] as int?;
            var account = db.Accounts.Find(acc_id);
            if (account == null) return RedirectToAction("Login");

            return View(account);
        }

        // 1. ACTION CẬP NHẬT PROFILE (UpdateProfileJS)
        [HttpPost]
        [AuthorizeCheck]
        // Thêm tham số DateTime? birthday và đổi gender thành bool? để tránh lỗi
        public JsonResult UpdateProfileJS(string firstName, string lastName, bool? gender, DateTime? birthday)
        {
            try
            {
                var acc_id = Session["acc_id"] as int?;
                if (acc_id == null) return Json(new { success = false, requireLogin = true, message = "Phiên đăng nhập hết hạn." });

                // Kiểm tra dữ liệu đầu vào
                if (gender == null) return Json(new { success = false, message = "Vui lòng chọn giới tính!" });
                if (birthday == null) return Json(new { success = false, message = "Vui lòng nhập ngày sinh!" });

                // --- KIỂM TRA TUỔI KHI CẬP NHẬT ---
                var today = DateTime.Today;
                var age = today.Year - birthday.Value.Year;
                if (birthday.Value.Date > today.AddYears(-age)) age--;

                if (age < 15)
                {
                    return Json(new { success = false, message = "Ngày sinh không hợp lệ. Bạn phải đủ 15 tuổi." });
                }
                // --------------------------------

                var account = db.Accounts.Find(acc_id);
                if (account != null)
                {
                    account.account_first_name = firstName;
                    account.account_last_name = lastName;

                    // Cập nhật giới tính và ngày sinh
                    account.account_gender = gender.Value;
                    account.account_date = birthday.Value;

                    db.SaveChanges();
                    Session["acc_name"] = lastName + " " + firstName;

                    return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
                }
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        // 2. API Xác thực mật khẩu cũ (Dùng để mở khóa form)
        [HttpPost]
        [AuthorizeCheck]
        public JsonResult VerifyPasswordJS(string oldPassword)
        {
            var acc_id = Session["acc_id"] as int?;
            var account = db.Accounts.Find(acc_id);
            var hash_oldPassword = Utility.Encrypt(oldPassword);

            if (account != null && account.account_password == hash_oldPassword) // Nhớ hash nếu có
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Mật khẩu cũ không chính xác!" });
        }

        // 3. API Cập nhật Bảo mật (Email & Pass mới)
        [HttpPost]
        [AuthorizeCheck]
        public JsonResult UpdateSecurityJS(string newEmail, string newPassword)
        {
            try
            {
                var acc_id = Session["acc_id"] as int?;
                var account = db.Accounts.Find(acc_id);
               

                if (account != null)
                {
                    // Cập nhật Email (nếu có thay đổi)
                    if (!string.IsNullOrEmpty(newEmail) && newEmail != account.account_email)
                    {
                        // Kiểm tra email trùng lặp trước khi lưu
                        if (db.Accounts.Any(a => a.account_email == newEmail && a.account_id != acc_id))
                        {
                            return Json(new { success = false, message = "Email này đã được sử dụng bởi tài khoản khác!" });
                        }
                        account.account_email = newEmail;
                    }

                    // Cập nhật Mật khẩu mới (nếu người dùng có nhập)
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        var newHashPassword = Utility.Encrypt(newPassword);
                        account.account_password = newHashPassword;
                    }

                    db.SaveChanges();
                    return Json(new { success = true, message = "Cập nhật bảo mật thành công!" });
                }
                return Json(new { success = false, message = "Lỗi hệ thống." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}