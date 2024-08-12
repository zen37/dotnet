using Microsoft.EntityFrameworkCore;

namespace MyNamespace
{
    public class AppDbContext : DbContext
    {
        public DbSet<Header> Headers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<SubItem> SubItems { get; set; } 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //configure the one-to-many relationship between Header and Item
            modelBuilder.Entity<Header>()
                .HasMany(g => g.Items)
                .WithOne(cs => cs.Header)
                .HasForeignKey(cs => cs.HeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            //configure the one-to-many relationship between Item and SubItem
            modelBuilder.Entity<Item>()
                .HasMany(cs => cs.SubItems)
                .WithOne(c => c.Item)
                .HasForeignKey(c => c.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
