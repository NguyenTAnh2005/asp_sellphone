using System;
using System.ComponentModel.DataAnnotations;

namespace old_phone.Models // Namespace phải khớp với Stock.cs gốc
{
    [MetadataType(typeof(StockMetaData))]
    public partial class Stock
    {
        // Để trống để merge
    }

    public class StockMetaData
    {
        [Display(Name = "Sản phẩm biến thể")]
        public int variant_id { get; set; }

        [Display(Name = "Số lượng tồn kho")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(0, 10000, ErrorMessage = "Số lượng tồn phải từ 0 đến 10.000.")]
        public int stock_count { get; set; }
    }
}