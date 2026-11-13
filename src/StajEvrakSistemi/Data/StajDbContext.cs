
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

    public DbSet<Basvuru> Basvurular => Set<Basvuru>();
    public DbSet<BasvuruDosyasi> BasvuruDosyalari => Set<BasvuruDosyasi>();
    public DbSet<DogrulamaSonucu> DogrulamaSonuclari => Set<DogrulamaSonucu>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OgrenciKaydi>()
            .HasIndex(o => o.OgrenciNo)
            .IsUnique();
            modelBuilder.Entity<Basvuru>()
                .HasOne(b => b.OgrenciKaydi)
                .WithMany()
                .HasForeignKey(b => b.OgrenciKaydiId)
                .OnDelete(DeleteBehavior.Restrict);
    }
}
