using old_phone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace old_phone.ViewModels
{
    public class OrderItemViewModel
    {
        public int variant_id { get; set; }
        public string name { get; set; }
        public int ram { get; set; }
        public int rom { get; set; }
        public int count { get; set; }
        public decimal price { get; set; }
        public string image_url { get; set; }
    }
    public class OrderViewModel
    {
        public List<Hotline> listHotlines { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }

        // Cac thuoc tinh nhan du lieu tu form post len
        public int Order_HotlineId { get; set; }
        public string Order_TypePayment { get; set; }
    }
}