using demo.Models;
using demo.Models.ViewModels;
using demo.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Thêm thư viện để sử dụng HttpContext

namespace demo.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _dataContext.Products.ToListAsync(); // Lấy tất cả sản phẩm từ cơ sở dữ liệu
            return View(products); // Trả về tất cả sản phẩm
        }


        public async Task<IActionResult> Search(string searchTerm, string priceRange)
        {
            var query = _dataContext.Products.AsQueryable();

            // Lọc theo tên hoặc mô tả sản phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            // Lọc theo giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                var priceLimits = priceRange.Split('-').Select(x => decimal.Parse(x)).ToList();
                if (priceLimits.Count == 2)
                {
                    var minPrice = priceLimits[0];
                    var maxPrice = priceLimits[1];
                    query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
            }

            var products = await query.ToListAsync();

            ViewBag.Keyword = searchTerm;
            return View(products);
        }


        [HttpGet("product/detail/{id:long}")]
        public async Task<IActionResult> Detail(long id)
        {
            if (id <= 0) return RedirectToAction("Index");

            var productById = await _dataContext.Products
                .Include(p => p.Brand)    // Bao gồm thông tin thương hiệu
                .Include(p => p.Category) // Bao gồm thông tin danh mục
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productById == null)
            {
                return NotFound();
            }

            var relatedProducts = new List<ProductModel>();
            if (productById.CategoryId != null)
            {
                relatedProducts = await _dataContext.Products
                    .Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id)
                    .Take(4)
                    .ToListAsync();
            }

            var productRatings = await _dataContext.Ratings
                .Where(r => r.ProductId == id)
                .ToListAsync();

            // Lấy các câu hỏi của sản phẩm
            var questions = await _dataContext.Questions
                .Include(q => q.Answers)
                .Where(q => q.ProductId == id)
                .ToListAsync();

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productById,
                Ratings = productRatings,
                Questions = questions,
                BrandName = productById.Brand?.Name,  // Lấy tên thương hiệu
                CategoryName = productById.Category?.Name  // Lấy tên danh mục
            };

            ViewBag.RelatedProducts = relatedProducts;
            return View(viewModel);
        }


        // Đăng câu hỏi mới
        [HttpPost]
        public async Task<IActionResult> AskQuestion(long productId, string question, string userName)
        {
            // Kiểm tra câu hỏi không rỗng
            if (string.IsNullOrWhiteSpace(question))
            {
                TempData["error"] = "Câu hỏi không thể trống!";
                return RedirectToAction("Detail", new { id = productId });
            }

            // Kiểm tra tên người dùng không rỗng
            if (string.IsNullOrWhiteSpace(userName))
            {
                TempData["error"] = "Vui lòng nhập tên của bạn!";
                return RedirectToAction("Detail", new { id = productId });
            }

            // Kiểm tra cookie trong HttpContext (nếu cần)
            var userRoleId = HttpContext.Request.Cookies["RoleId"];

            // Tạo một đối tượng QuestionModel để lưu câu hỏi
            var questionModel = new QuestionModel
            {
                ProductId = productId,
                Question = question,
                UserName = userName, // Dùng tên người dùng nhập tay
                DateAsked = DateTime.Now,
                UserRoleId = userRoleId // Lưu thông tin vai trò nếu cần
            };

            // Thêm câu hỏi vào cơ sở dữ liệu
            _dataContext.Questions.Add(questionModel);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Câu hỏi của bạn đã được gửi!";
            return RedirectToAction("Detail", new { id = productId });
        }
        [HttpPost]
        public async Task<IActionResult> AnswerQuestion(long questionId, string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                TempData["error"] = "Câu trả lời không thể trống!";
                return RedirectToAction("Detail", new { id = questionId });
            }

            // Kiểm tra RoleId của admin từ cookie
            var roleId = HttpContext.Request.Cookies["RoleId"];
            if (roleId != "EA8006AF-4080-4458-9629-08F315AAD165")
            {
                TempData["error"] = "Bạn không có quyền trả lời câu hỏi!";
                return RedirectToAction("Detail", new { id = questionId });
            }

            // Tạo đối tượng trả lời
            var question = await _dataContext.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question != null)
            {
                var answerModel = new AnswerModel
                {
                    QuestionId = questionId,
                    Answer = answer,
                    AdminName = "Admin", // Thay thế bằng tên admin nếu cần
                    DateAnswered = DateTime.Now
                };

                // Lưu câu trả lời vào cơ sở dữ liệu
                _dataContext.Answers.Add(answerModel);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Câu trả lời đã được gửi!";
            }
            else
            {
                TempData["error"] = "Câu hỏi không tồn tại!";
            }

            return RedirectToAction("Detail", new { id = question.ProductId });
        }



        public async Task<IActionResult> BestSelling()
        {
            var bestSellingProducts = await _dataContext.Products
                .Where(p => p.SoldOut > 10)
                .ToListAsync();

            return View(bestSellingProducts);
        }

        public IActionResult HotProducts()
        {
            var products = _dataContext.Products
                .OrderByDescending(p => p.Id) // Lấy sản phẩm mới nhất dựa trên ID
                .Take(10)
                .ToList();

            return View(products);
        }
    }
}
