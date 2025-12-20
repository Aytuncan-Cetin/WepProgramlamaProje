using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SporSalonuYonetim.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UygulamaKullanicisi> _userManager;
        private readonly CultureInfo _culture = new CultureInfo("tr-TR");

        public RandevuController(ApplicationDbContext context, UserManager<UygulamaKullanicisi> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ÜYE: kendi randevularını görür
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var liste = await _context.Randevular
                .Where(r => r.UyeId == userId)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.Baslangic)
                .ToListAsync();

            return View(liste);
        }

        // GET: Randevu/Create
        public async Task<IActionResult> Create()
        {
            var vm = new RandevuOlusturViewModel
            {
                Antrenorler = await _context.Antrenorler.ToListAsync(),
                Hizmetler = await _context.Hizmetler.ToListAsync(),
                Tarih = DateTime.Today
            };

            return View(vm);
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RandevuOlusturViewModel model)
        {
            model.Antrenorler = await _context.Antrenorler.ToListAsync();
            model.Hizmetler = await _context.Hizmetler.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                ModelState.AddModelError("", "Oturum bilgileriniz bulunamadı.");
                return View(model);
            }

            var hizmet = await _context.Hizmetler.FindAsync(model.HizmetId);
            if (hizmet == null)
            {
                ModelState.AddModelError("", "Seçilen hizmet bulunamadı.");
                return View(model);
            }

            // Tarih + Saat birleştir
            DateTime baslangic;
            if (!DateTime.TryParseExact(
                    $"{model.Tarih:yyyy-MM-dd} {model.Saat}",
                    "yyyy-MM-dd HH:mm",
                    _culture,
                    DateTimeStyles.None,
                    out baslangic))
            {
                ModelState.AddModelError("", "Tarih veya saat formatı geçersiz.");
                return View(model);
            }

            DateTime bitis = baslangic.AddMinutes(hizmet.SureDakika);

            var gun = baslangic.DayOfWeek;
            var basTS = baslangic.TimeOfDay;
            var bitTS = bitis.TimeOfDay;

            // 1) Antrenörün o gün/saat aralığında tanımlı müsaitliği var mı?
            bool uygunMusaitlikVarMi = await _context.AntrenorMusaitlikler
                .AnyAsync(m =>
                    m.AntrenorId == model.AntrenorId &&
                    m.Gun == gun &&
                    m.BaslangicSaati <= basTS &&
                    m.BitisSaati >= bitTS);

            if (!uygunMusaitlikVarMi)
            {
                ModelState.AddModelError("", "Seçtiğiniz saat aralığı antrenör için müsait değil.");
                return View(model);
            }

            // 2) Çakışan randevu var mı?
            bool cakisanVar = await _context.Randevular
                .AnyAsync(r =>
                    r.AntrenorId == model.AntrenorId &&
                    r.Baslangic.Date == baslangic.Date &&
                    baslangic < r.Bitis &&
                    bitis > r.Baslangic);

            if (cakisanVar)
            {
                ModelState.AddModelError("", "Bu saat aralığında antrenörün başka bir randevusu var.");
                return View(model);
            }

            var randevu = new Randevu
            {
                UyeId = userId,
                AntrenorId = model.AntrenorId,
                HizmetId = model.HizmetId,
                Baslangic = baslangic,
                Bitis = bitis,
                Onaylandi = false
            };

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Seçilen antrenör + tarih + hizmet için uygun saatleri döner
        [HttpGet]
        public async Task<IActionResult> MusaitSaatler(int antrenorId, string tarih, int hizmetId)
        {
            if (antrenorId == 0 || string.IsNullOrWhiteSpace(tarih) || hizmetId == 0)
                return Json(Array.Empty<string>());

            if (!DateTime.TryParse(tarih, _culture, DateTimeStyles.None, out var gunTarih))
            {
                // Kullanıcı tarayıcısı farklı format gönderirse ikinci kez dene
                if (!DateTime.TryParse(tarih, out gunTarih))
                    return Json(Array.Empty<string>());
            }

            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);
            if (hizmet == null || hizmet.SureDakika <= 0)
                return Json(Array.Empty<string>());

            var gun = gunTarih.DayOfWeek;

            // O gün için antrenörün tanımlı tüm müsaitlik kayıtları
            var musaitlikler = await _context.AntrenorMusaitlikler
                .Where(m => m.AntrenorId == antrenorId && m.Gun == gun)
                .ToListAsync();

            if (!musaitlikler.Any())
                return Json(Array.Empty<string>());

            var sonucListesi = new System.Collections.Generic.List<string>();

            foreach (var m in musaitlikler)
            {
                // Slotları hizmet süresine göre böl
                var slotBas = gunTarih.Date + m.BaslangicSaati;
                var slotBitLimit = gunTarih.Date + m.BitisSaati;

                while (slotBas.AddMinutes(hizmet.SureDakika) <= slotBitLimit)
                {
                    var slotBitis = slotBas.AddMinutes(hizmet.SureDakika);

                    bool cakisanVar = await _context.Randevular
                        .AnyAsync(r =>
                            r.AntrenorId == antrenorId &&
                            r.Baslangic.Date == gunTarih.Date &&
                            slotBas < r.Bitis &&
                            slotBitis > r.Baslangic);

                    if (!cakisanVar)
                    {
                        // HH:mm formatı
                        sonucListesi.Add(slotBas.ToString("HH:mm"));
                    }

                    slotBas = slotBas.AddMinutes(hizmet.SureDakika);
                }
            }

            // Tekrar eden saatleri temizle, sıralayıp dön
            var distinctOrdered = sonucListesi.Distinct().OrderBy(x => x).ToList();
            return Json(distinctOrdered);
        }

        // ÜYE: kendi randevusunu iptal eder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id)
        {
            var userId = _userManager.GetUserId(User);

            var randevu = await _context.Randevular
                .FirstOrDefaultAsync(r => r.Id == id && r.UyeId == userId);

            if (randevu == null)
                return NotFound();

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ADMIN: tüm randevular
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var liste = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.Baslangic)
                .ToListAsync();

            return View(liste);
        }

        // ADMIN: randevu onaylama
        // ADMIN: randevu onaylama (link ile GET isteği)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Onayla(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r == null)
                return NotFound();

            r.Onaylandi = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AdminIndex));
        }


        // ADMIN: randevu silme
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminSil(int id)
        {
            var r = await _context.Randevular.FindAsync(id);
            if (r != null)
            {
                _context.Randevular.Remove(r);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
