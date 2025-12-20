using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Data
{
    public class ApplicationDbContext : IdentityDbContext<UygulamaKullanicisi>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SalonBilgi> SalonBilgileri { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikler { get; set; }
        public DbSet<AntrenorMusaitlik> AntrenorMusaitlikleri => AntrenorMusaitlikler;
        public DbSet<AntrenorHizmet> AntrenorHizmetleri { get; set; }   
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<YapayZekaOneriKaydi> YapayZekaOneriKayitlari { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Antrenor–Hizmet çoktan çoğa: composite key
            builder.Entity<AntrenorHizmet>()
                .HasKey(x => new { x.AntrenorId, x.HizmetId });

            builder.Entity<AntrenorHizmet>()
                .HasOne(x => x.Antrenor)
                .WithMany(a => a.AntrenorHizmetler)
                .HasForeignKey(x => x.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);   // => Cascade yok

            builder.Entity<AntrenorHizmet>()
                .HasOne(x => x.Hizmet)
                .WithMany(h => h.AntrenorHizmetler)
                .HasForeignKey(x => x.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);   // => Cascade yok

            // Randevu ilişkilerinde de cascade kapatalım, daha temiz olsun
            builder.Entity<Randevu>()
                .HasOne(r => r.Uye)
                .WithMany(u => u.Randevular)
                .HasForeignKey(r => r.UyeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Antrenor)
                .WithMany(a => a.Randevular)
                .HasForeignKey(r => r.AntrenorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Randevu>()
                .HasOne(r => r.Hizmet)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HizmetId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
