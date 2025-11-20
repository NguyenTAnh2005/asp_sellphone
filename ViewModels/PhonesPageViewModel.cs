using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using old_phone.Models;
using PagedList;

namespace old_phone.ViewModels
{
    public class PhonesPageViewModel
    {
        // Dữ liệu chính chứa DS sp 
        public IPagedList<Variant_Phone> Phones {  get; set; }
        // Danh sách select có sẵn: hãng, ram, rom, hệ điều hành
        public List<Company> Companies { get; set; }
        public List<int> RamOptions { get; set; }
        public List<int> RomOptions { get; set; }
        public List<string> OsOptions {  get; set; }
        // Các tham số lọc 
        public string SearchQuery { get; set; }
        public int? CompanyId { get; set; }
        public int? MinPrice {  get; set; }
        public int? MaxPrice {  get; set; }
        public string Sort { get; set; }
        public int? Ram {  get; set; }
        public int? Rom {  get; set; }
        public string Os { get; set; }

    }
}