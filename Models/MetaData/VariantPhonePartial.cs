using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    [MetadataType(typeof(VariantPhoneMetaData))]
    public partial class Variant_Phone
    {
        // Để trống
    }

    public class VariantPhoneMetaData
    {
        [Display(Name = "Dung lượng RAM (GB)")]
        [Required(ErrorMessage = "Nhập RAM.")]
        [Range(1, 128, ErrorMessage = "RAM không hợp lý (1-128GB).")]
        public int variant_ph_ram { get; set; }

        [Display(Name = "Bộ nhớ trong (GB)")]
        [Required(ErrorMessage = "Nhập ROM.")]
        [Range(1, 2048, ErrorMessage = "ROM không hợp lý.")]
        public int variant_ph_rom { get; set; }

        [Display(Name = "Màu sắc")]
        [Required(ErrorMessage = "Nhập màu sắc.")]
        [StringLength(50, ErrorMessage = "Tên màu tối đa 50 ký tự.")]
        public string variant_ph_color { get; set; }

        [Display(Name = "Giá gốc")]
        [Required(ErrorMessage = "Nhập giá gốc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm.")]
        [DataType(DataType.Currency)]
        public long variant_ph_org_price { get; set; }

        [Display(Name = "Giá mới (Niêm yết)")]
        [Required(ErrorMessage = "Nhập giá mới.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm.")]
        [DataType(DataType.Currency)]
        public long variant_ph_new_price { get; set; }

        [Display(Name = "Giá bán cuối cùng")]
        [Required(ErrorMessage = "Nhập giá bán cuối cùng.")]
        [DataType(DataType.Currency)]
        public long variant_ph_final_price { get; set; }

        [Display(Name = "Ảnh đại diện")]
        [Required(ErrorMessage = "Nhập đường dẫn ảnh đại diện.")]
        public string variant_ph_img { get; set; }

        [Display(Name = "Tình trạng sản phẩm")]
        [Required(ErrorMessage = "Nhập tình trạng sản phẩm.")]
        public string variant_ph_state { get; set; }


    }
}