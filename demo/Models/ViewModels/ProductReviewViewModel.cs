namespace demo.Models.ViewModels
{
    public class ProductReviewViewModel
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Comment { get; set; }
        public int Star { get; set; }  // Đánh giá sao từ 1-5
        public string Name { get; internal set; }
        public string Email { get; internal set; }
        public string Status { get; internal set; }
    }

}
