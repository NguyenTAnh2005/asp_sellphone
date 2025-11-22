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

        // Them moi hoac chinh sua 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddOrEdit(int? hotline_id, string hotline_name,string hotline_address,
                                      string hotline_phonenumber, bool hotline_default = false)
        {
            var acc_id = Session["acc_id"] as int?;
            if(hotline_default)
            {
                // set ta ca cac dia chi khac thanh false tai gia tri default neu nhu cap nhat hoac tao moi co xac nhan set default
                var allAddresses = db.Hotlines.Where(h => h.account_id == acc_id).ToList();
                foreach(var addr in allAddresses)
                {
                    addr.hotline_default = false;
                }
            }
            // luu thay doi
            // Neu nhu ko nhan duowc id hotline => dang tao moi 
            if (hotline_id == null || hotline_id == 0)
            {
                var newHotline = new Hotline()
                {
                    account_id = acc_id.Value,
                    hotline_name = hotline_name,
                    hotline_address = hotline_address,
                    hotline_phonenumber = hotline_phonenumber,
                    hotline_default = hotline_default
                };
                // Nếu đây là địa chỉ đầu tiên của user -> Tự động set mặc định
                if (!db.Hotlines.Any(a => a.account_id == acc_id)) newHotline.hotline_default = true;
                db.Hotlines.Add(newHotline);
                db.SaveChanges();
                TempData["Message"] = "Them dia chi thanh cong!";
                TempData["MsgType"] = "success"; // success, danger, warning, info
            }
            else
            {
                var existAddr = db.Hotlines.FirstOrDefault(a => a.hotline_id == hotline_id && a.account_id == acc_id);
                if(existAddr != null)
                {
                    existAddr.hotline_name = hotline_name;
                    existAddr.hotline_address = hotline_address;
                    existAddr.hotline_phonenumber = hotline_phonenumber;

                    if(hotline_default)
                    {
                        existAddr.hotline_default = true;
                    }
                    db.SaveChanges();
                    TempData["Message"] = "Cap nhat dia chi thanh cong!";
                    TempData["MsgType"] = "success"; // success, danger, warning, info
                }
            }
            return RedirectToAction("Index");
        }

        // XÓA ĐỊA CHỈ
        public ActionResult Delete(int id)
        {
            var acc_id = Session["acc_id"] as int?;
            var addr = db.Hotlines.FirstOrDefault(a => a.hotline_id == id && a.account_id == acc_id);

            if (addr != null)
            {
                if (addr.hotline_default == true)
                {
                    TempData["Message"] = "Không thể xóa địa chỉ mặc định!";
                    TempData["MsgType"] = "error"; // success, danger, warning, info
                }
                else
                {
                    db.Hotlines.Remove(addr);
                    db.SaveChanges();
                    TempData["Message"] = "Xóa dia chi thanh cong!";
                    TempData["MsgType"] = "success"; 
                }
            }
            return RedirectToAction("Index");
        }

        // SET MẶC ĐỊNH 
        public ActionResult SetDefault(int id)
        {
            var acc_id = Session["acc_id"] as int?;
            // 1. Reset hết
            var all = db.Hotlines.Where(a => a.account_id == acc_id).ToList();
            all.ForEach(a => a.hotline_default = false);

            // 2. Set cái được chọn
            var target = all.FirstOrDefault(a => a.hotline_id == id);
            if (target != null) target.hotline_default = true;

            db.SaveChanges();
            TempData["Message"] = "Đã thay đổi địa chỉ mặc định.";
            TempData["MsgType"] = "success";
            return RedirectToAction("Index");
        }
    }
}