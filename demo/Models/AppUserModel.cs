using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occuapation { get; set; }
        public string RoleId { get; set; }
        public string Token { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public override string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng 1")]
        public string ShippingAddress1 { get; set; }

        public string ShippingAddress2 { get; set; } // Có thể để trống nếu không cần thiết

        // Thêm thuộc tính số lần mua hàng
        public int PurchaseCount { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
    }
}
