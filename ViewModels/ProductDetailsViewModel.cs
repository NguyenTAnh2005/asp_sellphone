using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using old_phone.Models;

namespace old_phone.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public Phone Phone { get; set; }
        public Variant_Phone Variant { get; set; }
        public List<string> ListImages {  get; set; }
        public int StockCount { get; set; }
    }
}