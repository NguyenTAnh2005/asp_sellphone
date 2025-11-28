using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.ViewModels
{
    public class ManageSaleViewModel
    {
        public int sale_id { get; set; }

        [Display(Name = "Tên chương trình")]
        [Required(ErrorMessage = "Vui lòng nhập tên chương trình.")]
        public string sale_name { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [Required]
        public DateTime sale_start { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [Required]
        public DateTime sale_end { get; set; }

        [Display(Name = "Sản phẩm áp dụng")]
        [Required]
        public int variant_id { get; set; }

        // --- CÁC TRƯỜNG PHỤ ĐỂ HIỂN THỊ & NHẬP GIÁ ---

        [Display(Name = "Giá Niêm Yết (Hiện tại)")]
        public long CurrentNewPrice { get; set; } // Chỉ để hiển thị (Readonly)

        [Display(Name = "Giá Sau Giảm (Final Price)")]
        [Required(ErrorMessage = "Vui lòng nhập giá sau giảm.")]
        [Range(1, long.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn 0.")]
        public long DiscountPrice { get; set; } // Đây là giá sẽ lưu vào variant_ph_final_price
    }
}