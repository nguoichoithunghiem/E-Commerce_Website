using demo.Models;
using demo.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace demo.Controllers
{
    public class RatingController : Controller
    {
        private readonly DataContext _dataContext;

        public RatingController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Hiển thị trang đánh giá cho sản phẩm
        public IActionResult Create(int productId)
        {
            // Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa
            var existingRating = _dataContext.Ratings
                .FirstOrDefault(r => r.ProductId == productId && r.Status == "Đã đánh giá");

            // Nếu người dùng đã đánh giá rồi, không cho phép tạo đánh giá mới
            if (existingRating != null)
            {
                TempData["error"] = "Bạn đã đánh giá sản phẩm này rồi.";
                return RedirectToAction("ProductDetails", "Product", new { id = productId });
            }

            var model = new RatingModel
            {
                ProductId = productId
            };
            return View(model);
        }

        // Xử lý việc gửi đánh giá
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RatingModel ratingModel)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra nếu người dùng đã đánh giá sản phẩm này rồi
                var existingRating = await _dataContext.Ratings
                    .FirstOrDefaultAsync(r => r.ProductId == ratingModel.ProductId && r.Email == ratingModel.Email);

                if (existingRating != null)
                {
                    TempData["error"] = "Bạn đã đánh giá sản phẩm này rồi.";
                    return RedirectToAction("ProductDetails", "Product", new { id = ratingModel.ProductId });
                }

                // Thêm đánh giá mới và thay đổi trạng thái
                ratingModel.Status = "Đã đánh giá"; // Đánh dấu là đã đánh giá
                _dataContext.Ratings.Add(ratingModel);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Đánh giá của bạn đã được gửi thành công.";
                return RedirectToAction("ProductDetails", "Product", new { id = ratingModel.ProductId });
            }
            return View(ratingModel);
        }
    }
}
