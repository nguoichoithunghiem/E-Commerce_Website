using demo.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly DataContext _dataContext;

        public ReportController(DataContext context)
        {
            _dataContext = context;
        }

        // Thống kê doanh thu theo khoảng thời gian
        [HttpGet]
        [Route("Admin/Report/Revenue")]
        public async Task<IActionResult> RevenueReport(DateTime? FromDate, DateTime? ToDate)
        {
            var ordersQuery = _dataContext.Orders.AsQueryable();

            // Lọc đơn hàng theo FromDate và ToDate
            if (FromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate >= FromDate.Value);
            }

            if (ToDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedDate <= ToDate.Value);
            }

            // Lấy danh sách đơn hàng sau khi lọc
            var orders = await ordersQuery.ToListAsync();

            // Tính tổng doanh thu trong khoảng thời gian đã lọc
            var totalRevenue = orders.Sum(o => o.TotalAfterDiscount);

            // Truyền tổng doanh thu vào ViewBag
            ViewBag.TotalRevenue = totalRevenue.ToString("#,##0 đ");

            // Trả về View với danh sách đơn hàng
            return View(orders);
        }



        // Xem chi tiết đơn hàng
        [HttpGet]
        [Route("Admin/Report/ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var detailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();

            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.ShippingCost = order.ShippingCost;
            ViewBag.Status = order.Status;
            return View(detailsOrder);
        }
    }
}
