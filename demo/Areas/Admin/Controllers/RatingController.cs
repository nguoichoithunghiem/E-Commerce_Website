using demo.Models;
using demo.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RatingController : Controller
    {
        private readonly DataContext _dataContext;

        public RatingController(DataContext context)
        {
            _dataContext = context;
        }

        // Route for Index
        [Route("admin/ratings/index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<RatingModel> ratings = await _dataContext.Ratings
                .Include(r => r.Product) // If you want to include Product details
                .ToListAsync();
            const int pageSize = 10;

            if (pg < 1)
            {
                pg = 1;
            }

            int recsCount = ratings.Count();
            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;
            var data = ratings.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }

        // Route for Create (GET)
        [Route("admin/ratings/create")]
        public IActionResult Create()
        {
            return View();
        }

        // Route for Create (POST)
        [Route("admin/ratings/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                // Set the status for the rating if needed (e.g., Pending or Approved)
                rating.Status = "Pending"; // Example status

                _dataContext.Add(rating);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm đánh giá sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Có lỗi trong mẫu dữ liệu!";
                string errorMessage = string.Join("\n", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(errorMessage);
            }
        }

        // Route for Edit (GET)
        [Route("admin/ratings/edit/{Id}")]
        public async Task<IActionResult> Edit(int Id)
        {
            RatingModel rating = await _dataContext.Ratings.FindAsync(Id);
            if (rating == null)
            {
                TempData["error"] = "Không tìm thấy đánh giá";
                return RedirectToAction("Index");
            }

            return View(rating);
        }

        // Route for Edit (POST)
        [Route("admin/ratings/edit/{Id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                _dataContext.Update(rating);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật đánh giá thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Có lỗi trong mẫu dữ liệu!";
                string errorMessage = string.Join("\n", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(errorMessage);
            }
        }

        // Route for Delete
        [Route("admin/ratings/delete/{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            RatingModel rating = await _dataContext.Ratings.FindAsync(Id);

            if (rating != null)
            {
                _dataContext.Ratings.Remove(rating);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Đánh giá đã được xóa thành công";
            }
            else
            {
                TempData["error"] = "Không tìm thấy đánh giá";
            }

            return RedirectToAction("Index");
        }
    }
}
