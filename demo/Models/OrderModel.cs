namespace demo.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string PaymentMethod { get; internal set; }

        // Bổ sung thông tin giao hàng
        public string ShippingAddress { get; set; } // Địa chỉ giao hàng
        public string PhoneNumber { get; set; } // Số điện thoại liên hệ

        // Liên kết với OrderDetail (danh sách chi tiết đơn hàng)
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public decimal TotalAmount { get; set; } // Tổng gốc của đơn hàng
        public decimal TotalAfterDiscount { get; set; } // Tổng sau khi giảm gi
    }
}
