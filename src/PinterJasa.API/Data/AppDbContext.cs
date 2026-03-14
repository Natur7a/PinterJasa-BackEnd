using Microsoft.EntityFrameworkCore;
using PinterJasa.API.Models;

namespace PinterJasa.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PlatformConfig> PlatformConfigs => Set<PlatformConfig>();
    public DbSet<LocationPing> LocationPings => Set<LocationPing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20);
            e.Property(u => u.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
            e.Property(u => u.AvatarUrl).HasColumnName("avatar_url");
            e.Property(u => u.IsVerified).HasColumnName("is_verified");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at");
        });

        // Provider
        modelBuilder.Entity<Provider>(e =>
        {
            e.ToTable("providers");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UserId).HasColumnName("user_id");
            e.Property(p => p.Bio).HasColumnName("bio");
            e.Property(p => p.City).HasColumnName("city").HasMaxLength(100);
            e.Property(p => p.AverageRating).HasColumnName("average_rating").HasPrecision(3, 2);
            e.Property(p => p.TotalReviews).HasColumnName("total_reviews");
            e.Property(p => p.IsActive).HasColumnName("is_active");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.BankCode).HasColumnName("bank_code").HasMaxLength(20);
            e.Property(p => p.BankAccountNumber).HasColumnName("bank_account_number").HasMaxLength(30);
            e.Property(p => p.BankAccountName).HasColumnName("bank_account_name").HasMaxLength(100);
            e.HasOne(p => p.User)
                .WithOne(u => u.Provider)
                .HasForeignKey<Provider>(p => p.UserId);
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(c => c.Description).HasColumnName("description");
            e.Property(c => c.IconUrl).HasColumnName("icon_url");
            e.Property(c => c.CommissionRate).HasColumnName("commission_rate").HasPrecision(5, 4);
            e.Property(c => c.IsActive).HasColumnName("is_active");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
        });

        // Service
        modelBuilder.Entity<Service>(e =>
        {
            e.ToTable("services");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.ProviderId).HasColumnName("provider_id");
            e.Property(s => s.CategoryId).HasColumnName("category_id");
            e.Property(s => s.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            e.Property(s => s.Description).HasColumnName("description");
            e.Property(s => s.Price).HasColumnName("price").HasPrecision(12, 2);
            e.Property(s => s.PriceUnit).HasColumnName("price_unit").HasMaxLength(20);
            e.Property(s => s.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(s => s.CreatedAt).HasColumnName("created_at");
            e.Property(s => s.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(s => s.Provider)
                .WithMany(p => p.Services)
                .HasForeignKey(s => s.ProviderId);
            e.HasOne(s => s.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.CategoryId);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasColumnName("id");
            e.Property(o => o.CustomerId).HasColumnName("customer_id");
            e.Property(o => o.ServiceId).HasColumnName("service_id");
            e.Property(o => o.ProviderId).HasColumnName("provider_id");
            e.Property(o => o.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(o => o.TotalPrice).HasColumnName("total_price").HasPrecision(12, 2);
            e.Property(o => o.Address).HasColumnName("address");
            e.Property(o => o.Notes).HasColumnName("notes");
            e.Property(o => o.ScheduledAt).HasColumnName("scheduled_at");
            e.Property(o => o.CompletedAt).HasColumnName("completed_at");
            e.Property(o => o.CreatedAt).HasColumnName("created_at");
            e.Property(o => o.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Service)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.ServiceId);
            e.HasOne(o => o.Provider)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.OrderId).HasColumnName("order_id");
            e.Property(p => p.CustomerId).HasColumnName("customer_id");
            e.Property(p => p.Amount).HasColumnName("amount").HasPrecision(12, 2);
            e.Property(p => p.Method).HasColumnName("method").HasMaxLength(30);
            e.Property(p => p.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(p => p.GatewayRef).HasColumnName("gateway_ref");
            e.Property(p => p.PaidAt).HasColumnName("paid_at");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.XenditInvoiceId).HasColumnName("xendit_invoice_id");
            e.Property(p => p.XenditInvoiceUrl).HasColumnName("xendit_invoice_url");
            e.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId);
            e.HasOne(p => p.Customer)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payout
        modelBuilder.Entity<Payout>(e =>
        {
            e.ToTable("payouts");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.OrderId).HasColumnName("order_id");
            e.Property(p => p.ProviderId).HasColumnName("provider_id");
            e.Property(p => p.GrossAmount).HasColumnName("gross_amount").HasPrecision(12, 2);
            e.Property(p => p.CommissionRate).HasColumnName("commission_rate").HasPrecision(5, 4);
            e.Property(p => p.CommissionAmount).HasColumnName("commission_amount").HasPrecision(12, 2);
            e.Property(p => p.NetAmount).HasColumnName("net_amount").HasPrecision(12, 2);
            e.Property(p => p.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(p => p.PaidAt).HasColumnName("paid_at");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.XenditDisbursementId).HasColumnName("xendit_disbursement_id");
            e.HasOne(p => p.Order)
                .WithOne(o => o.Payout)
                .HasForeignKey<Payout>(p => p.OrderId);
            e.HasOne(p => p.Provider)
                .WithMany(prov => prov.Payouts)
                .HasForeignKey(p => p.ProviderId);
        });

        // Review
        modelBuilder.Entity<Review>(e =>
        {
            e.ToTable("reviews");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.OrderId).HasColumnName("order_id");
            e.Property(r => r.ReviewerId).HasColumnName("reviewer_id");
            e.Property(r => r.ProviderId).HasColumnName("provider_id");
            e.Property(r => r.Rating).HasColumnName("rating");
            e.Property(r => r.Comment).HasColumnName("comment");
            e.Property(r => r.CreatedAt).HasColumnName("created_at");
            e.HasIndex(r => r.OrderId).IsUnique();
            e.HasOne(r => r.Order)
                .WithOne(o => o.Review)
                .HasForeignKey<Review>(r => r.OrderId);
            e.HasOne(r => r.Reviewer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Provider)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PlatformConfig
        modelBuilder.Entity<PlatformConfig>(e =>
        {
            e.ToTable("platform_config");
            e.HasKey(p => p.Key);
            e.Property(p => p.Key).HasColumnName("key").HasMaxLength(50);
            e.Property(p => p.Value).HasColumnName("value").HasMaxLength(255).IsRequired();
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.HasData(new PlatformConfig { Key = "default_commission_rate", Value = "0.15", UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        });

        // LocationPing
        modelBuilder.Entity<LocationPing>(e =>
        {
            e.ToTable("location_pings");
            e.HasKey(lp => lp.Id);
            e.Property(lp => lp.Id).HasColumnName("id");
            e.Property(lp => lp.OrderId).HasColumnName("order_id");
            e.Property(lp => lp.ProviderId).HasColumnName("provider_id");
            e.Property(lp => lp.Latitude).HasColumnName("latitude");
            e.Property(lp => lp.Longitude).HasColumnName("longitude");
            e.Property(lp => lp.AccuracyMeters).HasColumnName("accuracy_meters");
            e.Property(lp => lp.TimestampUtc).HasColumnName("timestamp_utc");
            e.HasOne(lp => lp.Order)
                .WithMany(o => o.LocationPings)
                .HasForeignKey(lp => lp.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(lp => lp.Provider)
                .WithMany()
                .HasForeignKey(lp => lp.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(lp => new { lp.OrderId, lp.TimestampUtc })
                .HasDatabaseName("IX_location_pings_order_id_timestamp_utc");
        });
    }
}
