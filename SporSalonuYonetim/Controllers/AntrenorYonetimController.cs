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
    public class AntrenorYonetimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorYonetimController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AntrenorYonetim
        public async Task<IActionResult> Index()
        {
            var liste = await _context.Antrenorler
                .Include(a => a.Salon)
                .ToListAsync();

            return View(liste);
        }

        // GET: /AntrenorYonetim/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetler)
                    .ThenInclude(ah => ah.Hizmet)
                .Include(a => a.Musaitliklar)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // GET: /AntrenorYonetim/Create
        public IActionResult Create()
        {
            ViewBag.SalonAdi = _context.SalonBilgileri.FirstOrDefault()?.Ad;
            return View(new Antrenor());
        }

        // POST: /AntrenorYonetim/Create
        [HttpPost]
        public async Task<IActionResult> Create(Antrenor antrenor)
        {
            // Tek salonu otomatik bağla
            var salon = await _context.SalonBilgileri.FirstOrDefaultAsync();
            if (salon != null)
                antrenor.SalonBilgiId = salon.Id;

            // ModelState.IsValid kontrolü YOK – direkt kaydediyoruz
            _context.Antrenorler.Add(antrenor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /AntrenorYonetim/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewBag.SalonAdi = (await _context.SalonBilgileri.FirstOrDefaultAsync())?.Ad;
            return View(antrenor);
        }

        // POST: /AntrenorYonetim/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Antrenor antrenor)
        {
            if (id != antrenor.Id) return NotFound();

            var salon = await _context.SalonBilgileri.FirstOrDefaultAsync();
            if (salon != null)
                antrenor.SalonBilgiId = salon.Id;

            // Burada da ModelState’e bakmadan güncelliyoruz
            _context.Antrenorler.Update(antrenor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /AntrenorYonetim/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // POST: /AntrenorYonetim/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                // İlişkili kayıtları da temizle
                var iliskiler = _context.AntrenorHizmetleri.Where(x => x.AntrenorId == id);
                _context.AntrenorHizmetleri.RemoveRange(iliskiler);

                var musaitliklar = _context.AntrenorMusaitlikleri.Where(x => x.AntrenorId == id);
                _context.AntrenorMusaitlikleri.RemoveRange(musaitliklar);

                var randevular = _context.Randevular.Where(x => x.AntrenorId == id);
                _context.Randevular.RemoveRange(randevular);

                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
