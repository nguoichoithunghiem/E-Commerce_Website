using System.ComponentModel.DataAnnotations;

namespace demo.Models.ViewModels
{
    public class UserInfoViewModel
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string ShippingAddress1 { get; set; }

        public string ShippingAddress2 { get; set; }

        public int PurchaseCount { get; set; }

        public decimal TotalSpent { get; set; }

        public string RoleName { get; set; } // Thêm thuộc tính RoleName

    }
}