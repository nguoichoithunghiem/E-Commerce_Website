using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }

        public long ProductId { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập đánh giá sản phẩm")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá sao phải trong khoảng từ 1 đến 5 sao.")]
        public int Star { get; set; }  // Lưu sao đánh giá dưới dạng số nguyên (1-5)

        public string Status { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
