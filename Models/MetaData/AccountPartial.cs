using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    [MetadataType(typeof(AccountPartial))]
    public partial class Account
    {

    }

    public class AccountPartial
    {
        [Display(Name = "Họ và tên đệm")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên đệm")]
        [StringLength(30, ErrorMessage = "Họ và tên đệm không được vượt quá 30 ký tự!")]
        public string account_first_name { get; set; }

        [Display(Name = "Tên")]
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [StringLength(30, ErrorMessage = "Tên không được vượt quá 30 ký tự!")]
        public string account_last_name { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        [StringLength(30, ErrorMessage = "Email không được vượt quá 30 ký tự!")]
        public string account_email { get; set; }

        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Vui lòng chọn giới tính.")]
        public bool account_gender { get; set; }

        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Vui lòng chọn ngày sinh.")]
        [DataType(DataType.Date)]
        public System.DateTime account_date { get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự!")]
        [DataType(DataType.Password)]
        public string account_password { get; set; }
    }
}