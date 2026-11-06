
using Microsoft.EntityFrameworkCore;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Data;

public class StajDbContext : DbContext
{
    public StajDbContext(
        DbContextOptions<StajDbContext> options)
        : base(options)
    {
    }

    public DbSet<OgrenciKaydi> OgrenciKayitlari
        => Set<OgrenciKaydi>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OgrenciKaydi>()
            .HasIndex(o => o.OgrenciNo)
            .IsUnique();
    }
}
