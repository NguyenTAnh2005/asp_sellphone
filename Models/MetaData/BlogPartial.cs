using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    [MetadataType(typeof(BlogMetaData))]
    public partial class Blog { }

    public class BlogMetaData
    {
        [Display(Name = "Tiêu đề bài viết")]
        [Required(ErrorMessage = "Nhập tiêu đề.")]
        public string blog_name { get; set; }

        [Display(Name = "Tác giả")]
        [Required(ErrorMessage = "Nhập tác giả.")]
        public string blog_author { get; set; }

        [Display(Name = "Link bài viết")]
        [Required(ErrorMessage = "Nhập link bài viết.")]
        [Url(ErrorMessage = "Link không hợp lệ.")]
        public string blog_link { get; set; }

        [Display(Name = "Ngày đăng")]
        [Required(ErrorMessage = "Chọn ngày đăng.")]
        [DataType(DataType.Date)]
        public System.DateTime blog_time { get; set; }

        [Display(Name = "Ảnh bìa")]
        [Required(ErrorMessage = "Chọn ảnh bìa.")]
        public string blog_img { get; set; }
    }
}