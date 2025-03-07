using demo.Models;
using demo.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using demo.Areas.Admin.Repository;
using Shopping_Tutorial.Services.Vnpay;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private readonly IVnPayService _vnPayService;
        private static readonly HttpClient client = new HttpClient();

        public CheckoutController(IEmailSender emailSender, DataContext context, IVnPayService vnPayService)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _vnPayService = vnPayService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
        {
            string userEmail = Request.Cookies["Email"];

            var ordercode = Guid.NewGuid().ToString();
            var orderItem = new OrderModel
            {
                OrderCode = ordercode,
                PaymentMethod = PaymentMethod switch
                {
                    "VnPay" => "VNpay",
                    "Momo" => "Momo",
                    _ => "COD"
                },
                UserName = userEmail,
                Status = 1,
                CreatedDate = DateTime.Now
            };

            // Lấy giá vận chuyển từ cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            orderItem.ShippingCost = shippingPriceCookie != null ? JsonConvert.DeserializeObject<decimal>(shippingPriceCookie) : 0;

            // Nhận mã giảm giá từ cookie
            orderItem.CouponCode = Request.Cookies["CouponTitle"];

            // Nhận thông tin địa chỉ và số điện thoại từ cookie
            orderItem.ShippingAddress = Request.Cookies["ShippingAddress1"] ?? "Chưa cung cấp";
            orderItem.PhoneNumber = Request.Cookies["PhoneNumber"] ?? "Chưa cung cấp";

            // Lấy thông tin khách hàng (User) từ cơ sở dữ liệu để kiểm tra Role
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            decimal discountPercentage = 0;

            // Kiểm tra vai trò người dùng và tính giảm giá
            if (user != null)
            {
                switch (user.RoleId)
                {
                    case "25fe2c12-4613-4ef2-938b-7801abd5bdad":
                        discountPercentage = 5;
                        break;
                    case "f9a1e678-5dd9-4199-b4f2-3ae4d83aeefd":
                        discountPercentage = 10;
                        break;
                    case "3bc6270b-d69a-43ce-9861-b0832a814299":
                        discountPercentage = 20;
                        break;
                    default:
                        discountPercentage = 0; // Không giảm nếu là New Customer
                        break;
                }
            }

            // Kiểm tra mã coupon và tính toán giảm giá thêm
            var couponCode = Request.Cookies["CouponTitle"];
            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _dataContext.Coupons.FirstOrDefaultAsync(c => c.Name == couponCode);
                if (coupon != null && coupon.Status == 1 && DateTime.Now <= coupon.DateExpired)
                {
                    // Giả sử coupon có thuộc tính DiscountPercentage
                    discountPercentage += coupon.DiscountPercentage;
                }
            }

            // Tính tổng tiền đơn hàng (TotalAmount)
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            decimal totalAmount = cartItems.Sum(x => x.Price * x.Quantity);  // Tổng gốc
            decimal totalAfterDiscount = totalAmount * (1 - discountPercentage / 100);  // Sau giảm giá

            // Cập nhật giá trị vào OrderItem
            orderItem.TotalAmount = totalAmount;
            orderItem.TotalAfterDiscount = totalAfterDiscount;

            _dataContext.Add(orderItem);
            await _dataContext.SaveChangesAsync();

            // Cập nhật số tiền đã chi tiêu của người dùng
            if (user != null)
            {
                user.TotalSpent += totalAfterDiscount;  // Cộng thêm số tiền đơn hàng vào TotalSpent của user
                _dataContext.Update(user);
            }

            // Tạo chi tiết đơn hàng
            foreach (var cart in cartItems)
            {
                var orderdetail = new OrderDetail
                {
                    UserName = userEmail,
                    OrderCode = ordercode,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Quantity = cart.Quantity,
                    Size = cart.Size,
                    Color = cart.Color,
                };

                // Cập nhật số lượng sản phẩm
                var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                product.Quantity -= cart.Quantity;
                product.SoldOut += cart.Quantity;
                _dataContext.Update(product);
                _dataContext.Add(orderdetail);
            }

            // Cập nhật số lần mua hàng của user
            if (user != null)
            {
                user.PurchaseCount += 1;
                _dataContext.Update(user);
            }

            await _dataContext.SaveChangesAsync();

            // Xóa giỏ hàng khỏi session
            HttpContext.Session.Remove("Cart");

            TempData["success"] = "Đơn hàng đã được tạo, vui lòng chờ duyệt đơn hàng.";
            return RedirectToAction("History", "Account");
        }






        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.VnPayResponseCode == "00") // Giao dịch thành công
            {
                var newVNpayInsert = new VNpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now,
                };
                _dataContext.Add(newVNpayInsert);
                await _dataContext.SaveChangesAsync();

                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod, PaymentId);
            }
            else
            {
                TempData["error"] = "Giao dịch VNpay không thành công";
                return RedirectToAction("Index", "Cart");
            }
            return View(response);
        }
    }
}
