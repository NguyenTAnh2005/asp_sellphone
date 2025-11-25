using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    [MetadataType(typeof(ProductMetaData))]
    public partial class Product
    {
    }

    public class ProductMetaData
    {
        [Display(Name = "Tên điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(100, ErrorMessage = "Tên quá dài (tối đa 100 ký tự).")]
        public string product_name { get; set; }

        [Display(Name = "Hãng sản xuất")]
        [Required(ErrorMessage = "Vui lòng chọn hãng.")]
        public Nullable<int> company_id { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả.")]
        [DataType(DataType.MultilineText)] // Hiển thị textarea
        public string product_desc { get; set; }
    }
}