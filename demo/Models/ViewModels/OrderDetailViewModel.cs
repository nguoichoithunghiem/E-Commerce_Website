namespace demo.ViewModels
{
    public class OrderDetailViewModel
    {
        public string OrderCode { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
        public string CouponCode { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public List<OrderDetailViewModelDetail> OrderDetails { get; set; }
    }

    public class OrderDetailViewModelDetail
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
