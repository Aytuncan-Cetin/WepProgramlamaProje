using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Data
{
    public static class VeriBaslangic
    {
        public static async Task VerileriOlusturAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UygulamaKullanicisi>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // MIGRATION YERİNE: Veritabanını yoksa oluştur
            await context.Database.EnsureCreatedAsync();

            // 1) Roller
            const string adminRol = "Admin";
            const string uyeRol = "Uye";

            if (!await roleManager.Roles.AnyAsync())
            {
                await roleManager.CreateAsync(new IdentityRole(adminRol));
                await roleManager.CreateAsync(new IdentityRole(uyeRol));
            }

            // 2) Admin kullanıcı
            const string adminEmail = "b211210308@sakarya.edu.tr"; // Hocanın istediği format
            const string adminSifre = "sau";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new UygulamaKullanicisi
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Ad = "Admin",
                    Soyad = "Kullanici",
                    Hedef = "Yönetici hesabı"
                };


                var sonuc = await userManager.CreateAsync(admin, adminSifre);
                if (sonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, adminRol);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(admin, adminRol))
                {
                    await userManager.AddToRoleAsync(admin, adminRol);
                }
            }

            // 3) Tek salon bilgisi
            if (!await context.SalonBilgileri.AnyAsync())
            {
                var salon = new SalonBilgi
                {
                    Ad = "FormPlus Spor Salonu",
                    Adres = "Sakarya Merkez",
                    AcilisSaati = new TimeSpan(8, 0, 0),
                    KapanisSaati = new TimeSpan(23, 0, 0)
                };

                context.SalonBilgileri.Add(salon);
                await context.SaveChangesAsync();
            }

            var mevcutSalon = await context.SalonBilgileri.FirstAsync();

            // 4) Hizmetler
            if (!await context.Hizmetler.AnyAsync())
            {
                var hizmetler = new[]
                {
                    new Hizmet
                    {
                        Ad = "Kişisel Antrenman (PT)",
                        SureDakika = 60,
                        Ucret = 400,
                        Kategori = "Bireysel",
                        SalonBilgiId = mevcutSalon.Id
                    },
                    new Hizmet
                    {
                        Ad = "Grup Fitness",
                        SureDakika = 45,
                        Ucret = 150,
                        Kategori = "Grup",
                        SalonBilgiId = mevcutSalon.Id
                    },
                    new Hizmet
                    {
                        Ad = "Pilates",
                        SureDakika = 50,
                        Ucret = 200,
                        Kategori = "Grup",
                        SalonBilgiId = mevcutSalon.Id
                    }
                };

                context.Hizmetler.AddRange(hizmetler);
                await context.SaveChangesAsync();
            }

            // 5) Antrenörler
            if (!await context.Antrenorler.AnyAsync())
            {
                var antrenor1 = new Antrenor
                {
                    AdSoyad = "Mehmet Demir",
                    UzmanlikAlanlari = "Kas geliştirme, güç antrenmanları",
                    Ozgecmis = "5 yıllık deneyimli kişisel antrenör.",
                    FotoUrl = "/images/antrenor1.jpg",
                    SalonBilgiId = mevcutSalon.Id
                };

                var antrenor2 = new Antrenor
                {
                    AdSoyad = "Ayşe Yıldız",
                    UzmanlikAlanlari = "Kilo verme, pilates, esneklik",
                    Ozgecmis = "Pilates ve fonksiyonel antrenman eğitmeni.",
                    FotoUrl = "/images/antrenor2.jpg",
                    SalonBilgiId = mevcutSalon.Id
                };

                context.Antrenorler.AddRange(antrenor1, antrenor2);
                await context.SaveChangesAsync();

                var tumHizmetler = await context.Hizmetler.ToListAsync();

                foreach (var hizmet in tumHizmetler)
                {
                    context.AntrenorHizmetleri.Add(new AntrenorHizmet
                    {
                        AntrenorId = antrenor1.Id,
                        HizmetId = hizmet.Id
                    });

                    context.AntrenorHizmetleri.Add(new AntrenorHizmet
                    {
                        AntrenorId = antrenor2.Id,
                        HizmetId = hizmet.Id
                    });
                }

                await context.SaveChangesAsync();

                var musaitliklar = new[]
                {
                    new AntrenorMusaitlik
                    {
                        AntrenorId = antrenor1.Id,
                        Gun = DayOfWeek.Monday,
                        BaslangicSaati = new TimeSpan(10, 0, 0),
                        BitisSaati = new TimeSpan(14, 0, 0)
                    },
                    new AntrenorMusaitlik
                    {
                        AntrenorId = antrenor1.Id,
                        Gun = DayOfWeek.Wednesday,
                        BaslangicSaati = new TimeSpan(16, 0, 0),
                        BitisSaati = new TimeSpan(20, 0, 0)
                    },
                    new AntrenorMusaitlik
                    {
                        AntrenorId = antrenor2.Id,
                        Gun = DayOfWeek.Tuesday,
                        BaslangicSaati = new TimeSpan(9, 0, 0),
                        BitisSaati = new TimeSpan(13, 0, 0)
                    },
                    new AntrenorMusaitlik
                    {
                        AntrenorId = antrenor2.Id,
                        Gun = DayOfWeek.Thursday,
                        BaslangicSaati = new TimeSpan(15, 0, 0),
                        BitisSaati = new TimeSpan(19, 0, 0)
                    }
                };

                context.AntrenorMusaitlikleri.AddRange(musaitliklar);
                await context.SaveChangesAsync();
            }
        }
    }
}
