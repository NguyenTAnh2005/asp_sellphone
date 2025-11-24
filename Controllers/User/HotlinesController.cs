using old_phone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using old_phone.Common;
namespace old_phone.Controllers.User
{
    public class HotlinesController : Controller
    {
        OldPhoneEntities db = new OldPhoneEntities();
        // GET: Hotlines
        [AuthorizeCheck]
        public ActionResult Index()
        {
            var acc_id = Session["acc_id"] as int?;
            var listAddress = db.Hotlines
                .Where(h => h.account_id == acc_id)
                .OrderByDescending(h => h.hotline_default)
                .ToList();

            return View(listAddress);
        }

        // --- CÁC HÀM API JSON (Xử lý ngầm) ---

        // 1. Thêm mới hoặc Chỉnh sửa (JSON)
        [HttpPost]
        // Bỏ ValidateAntiForgeryToken tạm thời nếu chưa cấu hình header Ajax, hoặc thêm vào Ajax
        public JsonResult AddOrEditJS(int? hotline_id, string hotline_name, string hotline_address, string hotline_phonenumber, bool hotline_default = false)
        {
            var acc_id = Session["acc_id"] as int?;
            if (acc_id == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            try
            {
                if (hotline_default)
                {
                    // Reset các địa chỉ khác
                    var allAddresses = db.Hotlines.Where(h => h.account_id == acc_id).ToList();
                    allAddresses.ForEach(a => a.hotline_default = false);
                }

                if (hotline_id == null || hotline_id == 0)
                {
                    // --- THÊM MỚI ---
                    var newHotline = new Hotline
                    {
                        account_id = acc_id.Value,
                        hotline_name = hotline_name,
                        hotline_address = hotline_address,
                        hotline_phonenumber = hotline_phonenumber,
                        hotline_default = hotline_default
                    };
                    // Nếu chưa có địa chỉ nào -> Tự động set default
                    if (!db.Hotlines.Any(a => a.account_id == acc_id)) newHotline.hotline_default = true;

                    db.Hotlines.Add(newHotline);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Thêm địa chỉ thành công!" });
                }
                else
                {
                    // --- CẬP NHẬT ---
                    var existAddr = db.Hotlines.FirstOrDefault(a => a.hotline_id == hotline_id && a.account_id == acc_id);
                    if (existAddr != null)
                    {
                        existAddr.hotline_name = hotline_name;
                        existAddr.hotline_address = hotline_address;
                        existAddr.hotline_phonenumber = hotline_phonenumber;
                        if (hotline_default) existAddr.hotline_default = true;

                        db.SaveChanges();
                        return Json(new { success = true, message = "Cập nhật địa chỉ thành công!" });
                    }
                    return Json(new { success = false, message = "Không tìm thấy địa chỉ này." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 2. Xóa địa chỉ (JSON)
        [HttpPost]
        public JsonResult DeleteJS(int id)
        {
            var acc_id = Session["acc_id"] as int?;
            if (acc_id == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            try
            {
                var addr = db.Hotlines.FirstOrDefault(a => a.hotline_id == id && a.account_id == acc_id);
                if (addr != null)
                {
                    if (addr.hotline_default == true)
                    {
                        return Json(new { success = false, message = "Không thể xóa địa chỉ mặc định!" });
                    }

                    db.Hotlines.Remove(addr);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Xóa địa chỉ thành công!" });
                }
                return Json(new { success = false, message = "Địa chỉ không tồn tại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 3. Đặt mặc định (JSON)
        [HttpPost]
        public JsonResult SetDefaultJS(int id)
        {
            var acc_id = Session["acc_id"] as int?;
            if (acc_id == null) return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            try
            {
                var all = db.Hotlines.Where(a => a.account_id == acc_id).ToList();
                all.ForEach(a => a.hotline_default = false);

                var target = all.FirstOrDefault(a => a.hotline_id == id);
                if (target != null)
                {
                    target.hotline_default = true;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã đặt làm địa chỉ mặc định." });
                }
                return Json(new { success = false, message = "Địa chỉ không tồn tại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}