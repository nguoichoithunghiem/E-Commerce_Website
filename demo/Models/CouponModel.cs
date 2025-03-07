﻿using System.ComponentModel.DataAnnotations;

namespace demo.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên coupon")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mô tả coupon")]
        public string Description { get; set; }


        [Required(ErrorMessage = "Yêu cầu nhập ngày bắt đầu")]

        public DateTime DateStart { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập ngày kết thúc")]


        public DateTime DateExpired { get; set; }

        [Required(ErrorMessage = "Yêu cầu số lượng coupon")]
        public int Quantity { get; set; }

        public int Status { get; set; }

        // Thêm thuộc tính DiscountPercentage
        [Range(0, 100, ErrorMessage = "Giảm giá phải nằm trong khoảng từ 0 đến 100")]
        public decimal DiscountPercentage { get; set; }


    }

}
