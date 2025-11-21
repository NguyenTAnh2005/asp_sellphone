using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using old_phone.Models;
using PagedList;

namespace old_phone.ViewModels
{
    public class SalesPageViewModel
    {
        public IPagedList<Sale> SalesList {  get; set; }
    }
}