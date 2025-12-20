using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SporSalonuYonetim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RaporController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RaporController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1) TÜM ANTRENÖRLERİ GETİR
        // GET: /api/rapor/antrenorler
        [HttpGet("antrenorler")]
        public async Task<IActionResult> TumAntrenorler()
        {
            var liste = await _context.Antrenorler
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.UzmanlikAlanlari
                })
                .ToListAsync();

            return Ok(liste);
        }

        // 2) BELİRLİ TARİH/SAATTE UYGUN ANTRENÖRLER
        // Örnek: /api/rapor/antrenorler-uygun?tarih=2025-12-12&saat=14:00
        [HttpGet("antrenorler-uygun")]
        public async Task<IActionResult> UygunAntrenorler(DateTime tarih, string saat)
        {
            // İstenen başlangıç zamanı
            if (!TimeSpan.TryParse(saat, out var time))
            {
                return BadRequest("Saat formatı hatalı. Örn: 14:00");
            }

            var baslangic = tarih.Date.Add(time);
            var bitis = baslangic.AddHours(1); // Varsayılan 1 saatlik slot

            // Bu saat aralığında randevusu OLMAYAN antrenörleri getir
            var uygunAntrenorler = await _context.Antrenorler
                .Where(a => !_context.Randevular.Any(r =>
                    r.AntrenorId == a.Id &&
                    (
                        (baslangic >= r.Baslangic && baslangic < r.Bitis) ||
                        (bitis > r.Baslangic && bitis <= r.Bitis)
                    )))
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.UzmanlikAlanlari
                })
                .ToListAsync();

            return Ok(uygunAntrenorler);
        }

        // 3) BİR ÜYENİN TÜM RANDEVULARI
        // Örnek: /api/rapor/uye-randevular?email=ogrencinumarasi@sakarya.edu.tr
        [HttpGet("uye-randevular")]
        public async Task<IActionResult> UyeRandevular(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email zorunludur.");

            // Kullanıcıyı bul
            var uye = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (uye == null)
                return NotFound("Bu email ile kayıtlı üye bulunamadı.");

            var randevular = await _context.Randevular
                .Where(r => r.UyeId == uye.Id)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.Baslangic)
                .Select(r => new
                {
                    r.Id,
                    Hizmet = r.Hizmet.Ad,
                    Antrenor = r.Antrenor.AdSoyad,
                    r.Baslangic,
                    r.Bitis,
                    r.Onaylandi
                })
                .ToListAsync();

            return Ok(randevular);
        }
    }
}
