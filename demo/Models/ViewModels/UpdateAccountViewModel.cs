using System.ComponentModel.DataAnnotations;

namespace demo.Models.ViewModels
{
    public class UpdateAccountViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập user name")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng 1")]
        public string ShippingAddress1 { get; set; }

        public string ShippingAddress2 { get; set; } // Có thể để trống nếu không cần thiết
    }
}