using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    // --- COMPANY ---
    [MetadataType(typeof(CompanyMetaData))]
    public partial class Company { }

    public class CompanyMetaData
    {
        [Display(Name = "Hãng sản xuất")]
        [Required(ErrorMessage = "Chọn hãng")]
        public int company_id { get; set; }

        [Display(Name = "Tên hãng")]
        [Required(ErrorMessage = "Nhập tên hãng.")]
        public string company_name { get; set; }

        [Display(Name = "Mô tả hãng")]
        [Required(ErrorMessage = "Nhập mô tả hãng.")]
        [DataType(DataType.MultilineText)]
        public string company_desc { get; set; }
    }
}