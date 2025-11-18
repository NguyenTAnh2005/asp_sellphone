using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using old_phone.Models;

namespace old_phone.ViewModels
{
    public class HomepageViewModel
    {
        public List<Variant_Phone> HotSales { get; set; }       // Danh sách Sale
        public List<Variant_Phone> FeaturedPhones { get; set; } // Danh sách Nổi bật
        public List<Blog> Blogs { get; set; }                   // Danh sách Blog
    }
}