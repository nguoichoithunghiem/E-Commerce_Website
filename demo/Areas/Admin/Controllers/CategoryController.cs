using demo.Models;
using demo.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demo.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Category/[action]")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;

        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }

        // Route cho action Index
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> categories = _dataContext.Categories.ToList(); // Lấy tất cả danh mục

            const int pageSize = 10; // Mỗi trang sẽ có 10 mục

            if (pg < 1)
            {
                pg = 1; // Đảm bảo trang phải >= 1
            }

            int recsCount = categories.Count(); // Đếm số lượng danh mục

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = categories.Skip(recSkip).Take(pager.PageSize).ToList(); // Lấy các danh mục theo trang

            ViewBag.Pager = pager; // Gửi pager lên View

            return View(data); // Trả về danh sách danh mục
        }

        // Route cho action Create
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // Route cho action Create (POST)
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                var existingSlug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);

                if (existingSlug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã có trong cơ sở dữ liệu");
                    return View(category);
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Danh mục đã được thêm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Có lỗi trong dữ liệu nhập vào";
                return View(category);
            }
        }

        // Route cho action Edit (GET)
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _dataContext.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["error"] = "Danh mục không tồn tại";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // Route cho action Edit (POST)
        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Danh mục đã được cập nhật thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Dữ liệu không hợp lệ";
                return View(category);
            }
        }

        // Route cho action Delete
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _dataContext.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["error"] = "Danh mục không tồn tại";
                return RedirectToAction("Index");
            }

            _dataContext.Categories.Remove(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Danh mục đã được xóa thành công";
            return RedirectToAction("Index");
        }
    }
}