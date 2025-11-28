using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    [MetadataType(typeof(HotlineMetaData))]
    public partial class Hotline
    {
    }

    public class HotlineMetaData
    {
        [Display(Name = "Tên người nhận")]
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        [StringLength(100, ErrorMessage = "Tên quá dài.")]
        public string hotline_name { get; set; }

        [Display(Name = "Địa chỉ nhận hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết.")]
        [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự.")]
        [DataType(DataType.MultilineText)]
        public string hotline_address { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập SĐT.")]
        // Regex cho SĐT Việt Nam: Bắt đầu bằng 0, theo sau là 9 chữ số
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "SĐT không hợp lệ (phải bắt đầu bằng 0 và có 10 số).")]
        public string hotline_phonenumber { get; set; }

        [Display(Name = "Địa chỉ mặc định")]
        public bool hotline_default { get; set; }
    }
}