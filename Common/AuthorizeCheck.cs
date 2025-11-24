using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace old_phone.Common
{
    public class AuthorizeCheck:ActionFilterAttribute
    {
        public int RequiredRole { get; set; } = 1; // 1: User, 2: Admin
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var id_Session = HttpContext.Current.Session["acc_id"];
            var role_Session = HttpContext.Current.Session["acc_role"];
            // Neu chua dang nhap thi chuyen huong ve trang dang nhap
            if (id_Session == null || role_Session == null)
            {
                filterContext.Controller.TempData["Message"] = "Vui lòng đăng nhập để tiếp tục!";
                filterContext.Controller.TempData["MsgType"] = "warning";
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new {
                            controller = "Account", action = "Login", area = ""
                        })
                    );
                return;
            }
            // Kiem tra quyen truy cap
            if(RequiredRole != 1)
            {
                int currentRole = int.Parse(role_Session.ToString());
                if(currentRole != RequiredRole && currentRole!=2)
                {
                    // Neu nhu quyen tren session khong phu hop voi quyen yeu cau hoac khong phai admin
                    filterContext.Controller.TempData["Message"] = "Bạn không có quyền truy cập trang này";
                    filterContext.Controller.TempData["MsgType"] = "error";
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new
                            {
                                controller = "Shop",
                                action = "Index",
                                area = ""
                            })
                        );
                    return;
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}