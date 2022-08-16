using BookStoreAPI.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Data
{
    public class BookStoreContext : IdentityDbContext<User>
    {
        public BookStoreContext(DbContextOptions<BookStoreContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cart
            builder.Entity<Cart>().HasIndex(cart => cart.UserId).IsUnique();

            builder.Entity<BookCart>().HasKey(bookCart => new {
                bookCart.CartId,
                bookCart.BookId
            });

            // Order
            builder.Entity<BookOrder>().HasKey(bookOrder => new {
                bookOrder.OrderId,
                bookOrder.BookId
            });
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<BookCart> BookCarts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<BookOrder> BookOrders { get; set; }
    }
}
