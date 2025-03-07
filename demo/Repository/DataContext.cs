using Microsoft.EntityFrameworkCore;
using demo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace demo.Repository
{
    public class DataContext: IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext > options): base(options)
        {

        }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }
        public DbSet<RatingModel> Ratings { get; set; }
        public DbSet<WishlistModel> Wishlists { get; set; }
        public DbSet<StatisticalModel> Statisticals { get; set; }
        public DbSet<ShippingModel> Shippings { get; set; }
        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<VNpayModel> VNInfos { get; set; }
        public DbSet<QuestionModel> Questions { get; set; }
        public DbSet<AnswerModel> Answers { get; set; }

    }
}
