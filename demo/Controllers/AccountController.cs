using demo.Models.ViewModels;
using demo.Models;
using demo.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using demo.Areas.Admin.Repository;
using System.Security.Claims;
using demo.ViewModels;

namespace Shopping_Tutorial.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        public AccountController(IEmailSender emailSender, UserManager<AppUserModel> userManage,
            SignInManager<AppUserModel> signInManager, DataContext context)
        {
            _userManage = userManage;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _dataContext = context;

        }
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email not found";
                return RedirectToAction("ForgotPass", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                //update token to user
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();
                var receiver = checkMail.Email;
                var subject = "Change password for user " + checkMail.Email;
                var message = "Click on link to change password " +
                    "<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token + "'>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }


            TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
            return RedirectToAction("ForgotPass", "Account");
        }
        public IActionResult ForgotPass()
        {
            return View();
        }
        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                //update user with new password and token
                string newtoken = Guid.NewGuid().ToString();
                // Hash the new password
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManage.UpdateAsync(checkuser);
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        public async Task<IActionResult> History(DateTime? startDate, DateTime? endDate)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var ordersQuery = _dataContext.Orders.Where(o => o.UserName == userEmail);

            // Apply date filter if provided
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate <= endDate.Value);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.Id)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderCode = o.OrderCode,
                    ShippingCost = o.ShippingCost,
                    TotalAmount = o.TotalAmount,
                    TotalAfterDiscount = o.TotalAfterDiscount,
                    CouponCode = o.CouponCode,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                })
                .ToListAsync();

            ViewBag.UserEmail = userEmail;
            return View(orders);
        }
        [HttpGet]
        public async Task<IActionResult> OrderDetail(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return NotFound();
            }

            // Retrieve the order based on the order code
            var order = await _dataContext.Orders
                .Where(o => o.OrderCode == orderCode)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            // Retrieve the order details for this order
            var orderDetails = await _dataContext.OrderDetails
                .Include(od => od.Product)  // Ensure to include the related product information
                .Where(od => od.OrderCode == orderCode)
                .ToListAsync();

            // Map OrderDetails to OrderDetailViewModelDetail
            var orderDetailViewModelDetails = orderDetails.Select(od => new OrderDetailViewModelDetail
            {
                Id = od.Id,
                ProductName = od.Product.Name,  // Assuming 'Product' has a 'Name' property
                Price = od.Price,
                Quantity = od.Quantity,
                Size = od.Size,
                Color = od.Color,
                TotalPrice = od.Quantity * od.Price  // Calculating the total price
            }).ToList();

            // Create the ViewModel and assign the transformed data
            var orderDetailViewModel = new OrderDetailViewModel
            {
                OrderCode = order.OrderCode,
                ShippingCost = order.ShippingCost,
                TotalAmount = order.TotalAmount,
                TotalAfterDiscount = order.TotalAfterDiscount,
                CouponCode = order.CouponCode,
                Status = order.Status,
                CreatedDate = order.CreatedDate,
                ShippingAddress = order.ShippingAddress,
                PhoneNumber = order.PhoneNumber,
                OrderDetails = orderDetailViewModelDetails  // Pass the mapped list to the ViewModel
            };

            return View(orderDetailViewModel);
        }





        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            // Kiểm tra nếu ordercode không có giá trị
            if (string.IsNullOrEmpty(ordercode))
            {
                TempData["error"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("History", "Account");
            }

            // Lấy đơn hàng theo mã đơn hàng
            var order = await _dataContext.Orders
                .Where(o => o.OrderCode == ordercode)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng để hủy.";
                return RedirectToAction("History", "Account");
            }

            if (order.Status == 5)  // Đơn hàng đã hủy rồi
            {
                TempData["error"] = "Đơn hàng này đã được hủy.";
                return RedirectToAction("History", "Account");
            }

            // Cập nhật trạng thái đơn hàng thành 'Đã hủy'
            order.Status = 5;
            _dataContext.Update(order);

            // Lấy chi tiết đơn hàng
            var orderDetails = _dataContext.OrderDetails
                .Where(od => od.OrderCode == ordercode)
                .ToList();

            // Cập nhật lại số lượng sản phẩm khi hủy đơn hàng
            foreach (var orderDetail in orderDetails)
            {
                var product = await _dataContext.Products
                    .Where(p => p.Id == orderDetail.ProductId)
                    .FirstOrDefaultAsync();

                if (product != null)
                {
                    product.Quantity += orderDetail.Quantity;  // Trả lại số lượng sản phẩm vào kho
                    product.SoldOut -= orderDetail.Quantity;  // Giảm số lượng đã bán
                    _dataContext.Update(product);
                }
            }

            // Giảm PurchaseCount của user đi 1
            var userEmail = order.UserName;  // Giả sử UserName là email user
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null && user.PurchaseCount > 0)
            {
                user.PurchaseCount -= 1;  // Giảm số lần mua hàng
                _dataContext.Update(user);
            }

            // Lưu lại thay đổi trong cơ sở dữ liệu
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Đơn hàng đã được hủy, số lượng sản phẩm và số lần mua hàng của bạn đã được cập nhật.";
            return RedirectToAction("History", "Account");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManage.FindByNameAsync(loginVM.Username);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                    if (result.Succeeded)
                    {
                        // ✅ Lưu thông tin vào Cookie
                        Response.Cookies.Append("PhoneNumber", user.PhoneNumber ?? "", new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) });
                        Response.Cookies.Append("ShippingAddress1", user.ShippingAddress1 ?? "", new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) });
                        Response.Cookies.Append("ShippingAddress2", user.ShippingAddress2 ?? "", new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) });
                        Response.Cookies.Append("Email", user.Email ?? "", new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) });
                        Response.Cookies.Append("RoleId", user.RoleId ?? "", new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7) });

                        return Redirect(loginVM.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
            }
            return View(loginVM);
        }



        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel
                {
                    UserName = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    ShippingAddress1 = user.ShippingAddress1,
                    ShippingAddress2 = user.ShippingAddress2,
                    RoleId = "f8acb807-23d4-4096-a9ba-0c7939b2cd9a" // Gán mặc định RoleId
                };

                IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    TempData["success"] = "Tạo thành viên thành công";
                    return Redirect("/account/login");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }
        public async Task<IActionResult> UserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManage.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Lấy vai trò của người dùng
            var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);

            var model = new UserInfoViewModel
            {
                TotalSpent = user.TotalSpent,
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ShippingAddress1 = user.ShippingAddress1,
                ShippingAddress2 = user.ShippingAddress2,
                PurchaseCount = user.PurchaseCount,
                RoleName = role?.Name // Lấy tên vai trò
            };

            return View(model);
        }

        public async Task<IActionResult> UpdateAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManage.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UpdateAccountViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ShippingAddress1 = user.ShippingAddress1,
                ShippingAddress2 = user.ShippingAddress2
            };

            return View(model);
        }

        // Phương thức để xử lý cập nhật thông tin tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(UpdateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManage.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.Username;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.ShippingAddress1 = model.ShippingAddress1;
                user.ShippingAddress2 = model.ShippingAddress2;

                var result = await _userManage.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["success"] = "Cập nhật thông tin thành công.";
                    return RedirectToAction("History", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // Phương thức để hiển thị trang cập nhật mật khẩu
        public IActionResult UpdatePassword()
        {
            return View();
        }

        // Phương thức để xử lý cập nhật mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManage.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManage.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["success"] = "Cập nhật mật khẩu thành công.";
                    return RedirectToAction("History", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        // Phương thức để hiển thị trang tìm kiếm
        public IActionResult SearchOrders()
        {
            return View();
        }

        // Phương thức xử lý tìm kiếm đơn hàng theo số điện thoại
        // Phương thức POST để xử lý tìm kiếm đơn hàng theo số điện thoại
        [HttpPost]
        public async Task<IActionResult> SearchOrders(string phoneNumber)
        {
            // Nếu không có số điện thoại
            if (string.IsNullOrEmpty(phoneNumber))
            {
                TempData["error"] = "Số điện thoại không được để trống.";
                return View(); // Trả về view với thông báo lỗi
            }

            // Tìm đơn hàng của khách hàng dựa trên số điện thoại
            var orders = await _dataContext.Orders
                .Where(o => o.PhoneNumber == phoneNumber)
                .OrderByDescending(o => o.CreatedDate) // Sắp xếp theo ngày tạo
                .ToListAsync();

            if (orders.Count == 0)
            {
                TempData["error"] = "Không tìm thấy đơn hàng nào với số điện thoại này.";
            }

            // Trả kết quả về view với danh sách đơn hàng
            return View(orders);
        }
        [HttpGet] // Phương thức GET để hiển thị form review
        public async Task<IActionResult> ReviewOrder(string ordercode)
        {
            var order = await _dataContext.Orders
                .Where(o => o.OrderCode == ordercode)
                .FirstOrDefaultAsync();

            if (order == null || order.Status != 4)  // Kiểm tra đơn hàng có tồn tại và đã được nhận
            {
                TempData["error"] = "Không tìm thấy đơn hàng hoặc đơn hàng chưa được nhận.";
                return RedirectToAction("History", "Account");
            }

            var orderDetails = await _dataContext.OrderDetails
                .Where(od => od.OrderCode == ordercode)
                .Include(od => od.Product)
                .ToListAsync();

            // Get the user's reviews and their corresponding status
            var userReviews = await _dataContext.Ratings
                .Where(r => r.Email == User.Identity.Name) // User's email for identifying reviews
                .Select(r => new { r.ProductId, r.Status }) // Include Status of review for each product
                .ToListAsync();

            // Create the model for the view, excluding already reviewed products
            var model = orderDetails
                .Where(od => !userReviews.Any(ur => ur.ProductId == od.ProductId)) // Only products that haven't been reviewed
                .Select(od => new ProductReviewViewModel
                {
                    ProductId = od.ProductId,
                    ProductName = od.Product != null ? od.Product.Name : "N/A",
                    Price = od.Price,
                    Quantity = od.Quantity,
                    Status = userReviews.FirstOrDefault(ur => ur.ProductId == od.ProductId)?.Status // Get status for each product
                }).ToList();

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(RatingModel review)
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(review.Comment) || review.Star <= 0 || string.IsNullOrWhiteSpace(review.Name) || string.IsNullOrWhiteSpace(review.Email))
            {
                TempData["error"] = "Vui lòng cung cấp đầy đủ thông tin đánh giá.";
                return RedirectToAction("ReviewOrder"); // Không cần truyền ordercode nữa
            }

            // Thêm thông tin đánh giá vào cơ sở dữ liệu
            review.Status = "Đã đánh giá"; // Set the status to "Đã đánh giá" (Already rated)

            _dataContext.Ratings.Add(review);

            try
            {
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Đánh giá của bạn đã được gửi thành công.";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Đã xảy ra lỗi khi gửi đánh giá: " + ex.Message;
            }

            return RedirectToAction("Index", "Home"); // Trở lại trang lịch sử đơn hàng
        }






        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();

            // Xóa một cookie cụ thể
            Response.Cookies.Delete("PhoneNumber");
            Response.Cookies.Delete("ShippingAddress");
            Response.Cookies.Delete("ShippingAddress1");
            Response.Cookies.Delete("ShippingAddress2");
            Response.Cookies.Delete(".AspNetCore.Antiforgery.jhi2goC9Ipo");
            Response.Cookies.Delete(".AspNetCore.Session");
            Response.Cookies.Delete("Email");
            Response.Cookies.Delete("RoleId");

            return Redirect(returnUrl);
        }




    }
}