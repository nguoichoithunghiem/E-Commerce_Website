namespace demo.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }
        public decimal GrandTotal { get; set; }


        public decimal ShippingPrice { get; set; }

        public string CouponCode { get; set; }

        public string Size { get; set; }   // New property for size
        public string Color { get; set; }  // New property for color
    }
}
