using demo.Models;
using System.ComponentModel.DataAnnotations;

namespace demo.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetails { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập bình luận sản phẩm")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập email")]
        public string Email { get; set; }
        public List<RatingModel> Ratings { get; internal set; }
        public object Questions { get; internal set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public bool IsAdmin { get; set; }

    }
}