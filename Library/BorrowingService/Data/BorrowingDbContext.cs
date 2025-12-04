using BorrowingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BorrowingService.Data;

public class BorrowingDbContext : DbContext
{
    public BorrowingDbContext(DbContextOptions<BorrowingDbContext> options) : base(options)
    {
    }
    public DbSet<Borrowing> Borrowings { get; set; }
    public DbSet<BorrowingItem>   BorrowingItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Borrowing>()
            .HasMany(b => b.BorrowingItems)
            .WithOne(b => b.Borrowing)
            .HasForeignKey(i => i.Id)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}