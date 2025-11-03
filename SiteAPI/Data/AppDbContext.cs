using Microsoft.EntityFrameworkCore;
using SiteAPI.Models;
namespace SiteAPI.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // User constraints
        b.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        b.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        // BlogPost mapping
        b.Entity<BlogPost>(e =>
        {
            e.ToTable("blog_posts");

            e.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(160);

            e.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(180);

            e.HasIndex(p => p.Slug)
                .IsUnique();

            e.Property(p => p.Description)
                .HasMaxLength(300);

            // timestamps (MySQL/Pomelo)
            e.Property(p => p.CreatedUtc)
                .HasDefaultValueSql("UTC_TIMESTAMP()");

            // concurrency token
            e.Property(p => p.RowVersion)
                .IsRowVersion(); // maps to BLOB/TIMESTAMP depending on provider

            // relationships
            e.HasOne(p => p.Author)
                .WithMany()                  // or .WithMany(u => u.BlogPosts) if you add a collection to User
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
