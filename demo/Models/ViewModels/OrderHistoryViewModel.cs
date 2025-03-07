using System;
using System.Collections.Generic;

namespace demo.Models.ViewModels
{
    public class OrderHistoryViewModel
    {
        public string OrderCode { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
        public string CouponCode { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public long ProductId { get; set; }  // Thêm ProductId vào đây
    }

}
