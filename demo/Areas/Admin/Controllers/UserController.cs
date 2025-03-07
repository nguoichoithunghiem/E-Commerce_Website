using demo.Models;
using demo.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UserController(DataContext context,
            UserManager<AppUserModel> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                        join r in _dataContext.Roles on u.RoleId equals r.Id into roles
                                        from r in roles.DefaultIfEmpty()
                                        select new
                                        {
                                            User = u,
                                            RoleName = r != null ? r.Name : "No Role"
                                        }).ToListAsync();

            bool isUpdated = false;

            foreach (var userWithRole in usersWithRoles)
            {
                var user = userWithRole.User;
                var roleName = userWithRole.RoleName;

                if (roleName != "Admin") // Chỉ cập nhật nếu không phải Admin
                {
                    string newRoleId = null;

                    if (user.TotalSpent < 10000000)  // Dưới 10 triệu
                        newRoleId = "f8acb807-23d4-4096-a9ba-0c7939b2cd9a";  // Role cho người chi tiêu dưới 10 triệu
                    else if (user.TotalSpent >= 10000000 && user.TotalSpent < 20000000)  // Từ 10 triệu đến dưới 20 triệu
                        newRoleId = "25fe2c12-4613-4ef2-938b-7801abd5bdad";  // Role cho người chi tiêu từ 10 triệu đến dưới 20 triệu
                    else if (user.TotalSpent >= 20000000 && user.TotalSpent < 30000000)  // Từ 20 triệu đến dưới 30 triệu
                        newRoleId = "f9a1e678-5dd9-4199-b4f2-3ae4d83aeefd";  // Role cho người chi tiêu từ 20 triệu đến dưới 30 triệu
                    else if (user.TotalSpent >= 30000000)  // Trên 30 triệu
                        newRoleId = "3bc6270b-d69a-43ce-9861-b0832a814299";  // Role cho người chi tiêu trên 30 triệu

                    // Nếu Role thay đổi, cập nhật Role của người dùng
                    if (newRoleId != null && user.RoleId != newRoleId)
                    {
                        user.RoleId = newRoleId;
                        isUpdated = true;
                    }
                }
            }

            if (isUpdated)
            {
                await _dataContext.SaveChangesAsync();
            }

            return View(usersWithRoles);
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                // Phân loại role dựa trên TotalSpent trước khi tạo người dùng
                string newRoleId = null;

                if (user.TotalSpent < 10000000)  // Dưới 10 triệu
                    newRoleId = "f8acb807-23d4-4096-a9ba-0c7939b2cd9a";  // Role cho người chi tiêu dưới 10 triệu
                else if (user.TotalSpent >= 10000000 && user.TotalSpent < 20000000)  // Từ 10 triệu đến dưới 20 triệu
                    newRoleId = "25fe2c12-4613-4ef2-938b-7801abd5bdad";  // Role cho người chi tiêu từ 10 triệu đến dưới 20 triệu
                else if (user.TotalSpent >= 20000000 && user.TotalSpent < 30000000)  // Từ 20 triệu đến dưới 30 triệu
                    newRoleId = "f9a1e678-5dd9-4199-b4f2-3ae4d83aeefd";  // Role cho người chi tiêu từ 20 triệu đến dưới 30 triệu
                else if (user.TotalSpent >= 30000000)  // Trên 30 triệu
                    newRoleId = "3bc6270b-d69a-43ce-9861-b0832a814299";  // Role cho người chi tiêu trên 30 triệu


                user.RoleId = newRoleId;  // Gán role cho người dùng mới

                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash); // Tạo user
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email); // Tìm user dựa vào email
                    var role = await _roleManager.FindByIdAsync(user.RoleId); // Lấy RoleId

                    // Gán quyền
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in createUserResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }

                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update other user properties (excluding password)
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            TempData["error"] = "Model validation failed.";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }

        private void AddIdentityErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }

            TempData["success"] = "User đã được xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
