using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    [MetadataType(typeof(SaleMetaData))]
    public partial class Sale
    {
    }

    public class SaleMetaData
    {
        [Display(Name = "Tên chương trình")]
        [Required(ErrorMessage = "Nhập tên khuyến mãi.")]
        public string sale_name { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [Required]
        [DataType(DataType.Date)]
        public System.DateTime sale_start { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [Required]
        [DataType(DataType.Date)]
        public System.DateTime sale_end { get; set; }

        [Display(Name = "Sản phẩm áp dụng")]
        [Required(ErrorMessage = "Chọn sản phẩm.")]
        public Nullable<int> variant_id { get; set; }
    }
}