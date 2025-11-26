using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models
{
    [MetadataType(typeof(PhoneMetaData))]
    public partial class Phone
    {
    }

    public class PhoneMetaData
    {
        [Display(Name = "Chip xử lý")]
        [Required(ErrorMessage = "Nhập tên Chip.")]
        public string phone_chip { get; set; }

        [Display(Name = "Kích thước màn hình")]
        [Required(ErrorMessage ="Vui lòng nhập kích thước màn")]
        public string phone_screen_size { get; set; }

        [Display(Name = "Camera trước")]
        [Required(ErrorMessage = "Vui lòng nhập thông tin camera trước")]
        public string phone_front_cam { get; set; }

        [Display(Name = "Camera sau")]
        [Required(ErrorMessage = "Vui lòng nhập thông tin camera sau")]
        public string phone_behind_cam { get; set; }

        [Display(Name = "Dung lượng Pin (mAh)")]
        [Required(ErrorMessage = "Vui lòng nhập dung lượng pin")]
        [Range(0, 20000, ErrorMessage = "Pin phải là số dương.")]
        public int phone_battery { get; set; }

        [Display(Name = "Hệ điều hành")]
        [Required(ErrorMessage = "Vui lòng nhập hệ điều hành")]
        public string phone_system { get; set; }

        [Display(Name = "Cổng sạc")]
        [Required(ErrorMessage = "Vui lòng nhập thông tin cổng sạc")]
        public string phone_charging_port { get; set; }

        [Display(Name = "Số lượng SIM")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng sim")]
        [Range(0, 5)]
        public int phone_sim_card { get; set; }

        [Display(Name = "Hỗ trợ NFC")]
        public bool phone_nfc { get; set; }

        [Display(Name = "Tai nghe (Jack/Type-C)")]
        [Required(ErrorMessage = "Vui lòng nhập thông tin tai nghe")]
        public string phone_ear_phone { get; set; }

        [Display(Name = "Hỗ trợ thẻ nhớ")]
        public bool phone_memory_card { get; set; }
    }
}