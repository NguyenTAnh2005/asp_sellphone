using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.ViewModels
{
    public class ManageVariant_PhoneViewModel
    {
        // --- Phần của Variant_Phone ---
        public int variant_id { get; set; }

        [Display(Name = "Sản Phẩm")]
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm.")]
        public int product_id { get; set; }

        [Display(Name = "RAM (GB)")]
        [Required]
        [Range(1, 128)]
        public int variant_ph_ram { get; set; }

        [Display(Name = "ROM (GB)")]
        [Required]
        [Range(1, 2048)]
        public int variant_ph_rom { get; set; }

        [Display(Name = "Màu sắc")]
        [Required]
        public string variant_ph_color { get; set; }

        [Display(Name = "Giá Gốc")]
        [Required]
        public long variant_ph_org_price { get; set; }

        [Display(Name = "Giá Mới")]
        [Required]
        public long variant_ph_new_price { get; set; }

        // Giá Final sẽ tự tính trong Controller
        public long variant_ph_final_price { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string variant_ph_img { get; set; }

        [Display(Name = "Trạng thái")]
        public string variant_ph_state { get; set; }

        // --- Phần MỚI của Stock (Kho) ---
        [Display(Name = "Số lượng nhập kho ban đầu")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng kho ban đầu.")]
        [Range(0, 10000, ErrorMessage = "Số lượng phải >= 0.")]
        public int InitialStockCount { get; set; }
    }
}