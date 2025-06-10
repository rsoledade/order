using Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Data.Context
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Order entity configuration
            modelBuilder.Entity<Domain.Entities.Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ExternalId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.OrderHash)
                    .HasMaxLength(255);

                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(500);

                // Convert OrderId value object to Guid
                entity.ComplexProperty(e => e.OrderId, p =>
                {
                    p.Property(o => o.Value)
                     .HasColumnName("OrderId")
                     .IsRequired();
                });

                // Convert Money value object to decimal
                entity.ComplexProperty(e => e.TotalAmount, p =>
                {
                    p.Property(m => m.Value)
                     .HasColumnName("TotalAmount")
                     .HasColumnType("decimal(18,2)")
                     .IsRequired();
                });

                // Create indexes
                entity.HasIndex(e => e.ExternalId).IsUnique();
                entity.HasIndex(e => e.OrderHash);
            });

            // Product entity configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Quantity)
                    .IsRequired();

                // Convert Money value object to decimal
                entity.ComplexProperty(e => e.Price, p =>
                {
                    p.Property(m => m.Value)
                     .HasColumnName("Price")
                     .HasColumnType("decimal(18,2)")
                     .IsRequired();
                });

                // Configure relationship with Order
                entity.HasOne(p => p.Order)
                    .WithMany(o => o.Products)
                    .HasForeignKey(p => p.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
