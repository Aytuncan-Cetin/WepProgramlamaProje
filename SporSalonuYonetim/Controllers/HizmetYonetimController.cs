using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HizmetYonetimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CultureInfo _culture = new CultureInfo("tr-TR");

        public HizmetYonetimController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /HizmetYonetim
        public async Task<IActionResult> Index()
        {
            var liste = await _context.Hizmetler
                .Include(h => h.Salon)
                .ToListAsync();

            return View(liste);
        }

        // GET: /HizmetYonetim/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // GET: /HizmetYonetim/Create
        public IActionResult Create()
        {
            ViewBag.SalonAdi = _context.SalonBilgileri.FirstOrDefault()?.Ad;
            return View(new Hizmet());
        }

        // POST: /HizmetYonetim/Create
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            // Form verilerini manuel alalım
            string ad = Request.Form["Ad"];
            string sureStr = Request.Form["SureDakika"];
            string ucretStr = Request.Form["Ucret"];
            string kategori = Request.Form["Kategori"];

            int.TryParse(sureStr, out int sureDakika);

            // Virgül veya noktalı formatı kabul et
            decimal ucret;
            if (!decimal.TryParse(ucretStr, NumberStyles.Any, _culture, out ucret))
            {
                decimal.TryParse(ucretStr, NumberStyles.Any, CultureInfo.InvariantCulture, out ucret);
            }

            var salon = await _context.SalonBilgileri.FirstOrDefaultAsync();

            var hizmet = new Hizmet
            {
                Ad = ad,
                SureDakika = sureDakika,
                Ucret = ucret,
                Kategori = kategori,
                SalonBilgiId = salon?.Id ?? 0
            };

            _context.Hizmetler.Add(hizmet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /HizmetYonetim/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            ViewBag.SalonAdi = (await _context.SalonBilgileri.FirstOrDefaultAsync())?.Ad;
            return View(hizmet);
        }

        // POST: /HizmetYonetim/Edit/5
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            // Formdan değerleri çek
            string ad = Request.Form["Ad"];
            string sureStr = Request.Form["SureDakika"];
            string ucretStr = Request.Form["Ucret"];
            string kategori = Request.Form["Kategori"];

            if (!int.TryParse(sureStr, out int sureDakika))
                sureDakika = hizmet.SureDakika;

            decimal ucret = hizmet.Ucret;
            if (!decimal.TryParse(ucretStr, NumberStyles.Any, _culture, out ucret))
            {
                decimal.TryParse(ucretStr, NumberStyles.Any, CultureInfo.InvariantCulture, out ucret);
            }

            var salon = await _context.SalonBilgileri.FirstOrDefaultAsync();

            // Alanları güncelle
            hizmet.Ad = ad;
            hizmet.SureDakika = sureDakika;
            hizmet.Ucret = ucret;
            hizmet.Kategori = kategori;
            hizmet.SalonBilgiId = salon?.Id ?? hizmet.SalonBilgiId;

            _context.Hizmetler.Update(hizmet);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /HizmetYonetim/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .Include(h => h.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // POST: /HizmetYonetim/DeleteConfirmed/5
        // Delete.cshtml içindeki form büyük ihtimalle asp-action="DeleteConfirmed" kullanıyor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Önce ilişkileri temizle
            var iliskiler = _context.AntrenorHizmetleri.Where(x => x.HizmetId == id);
            _context.AntrenorHizmetleri.RemoveRange(iliskiler);

            var randevular = _context.Randevular.Where(x => x.HizmetId == id);
            _context.Randevular.RemoveRange(randevular);

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
