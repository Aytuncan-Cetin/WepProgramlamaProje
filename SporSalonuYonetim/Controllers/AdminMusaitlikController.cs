using System;
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
    public class AdminMusaitlikController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMusaitlikController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TAKVİM EKRANI
        public async Task<IActionResult> Index(int antrenorId)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(a => a.Id == antrenorId);

            if (antrenor == null)
                return NotFound();

            ViewBag.AntrenorId = antrenor.Id;
            ViewBag.AntrenorAd = antrenor.AdSoyad;
            ViewBag.SalonAd = antrenor.Salon?.Ad;

            return View();
        }

        // FullCalendar için event listesi
        [HttpGet]
        public async Task<IActionResult> GetEvents(int antrenorId)
        {
            // Bu haftanın pazartesi tarihini bul
            var today = DateTime.Today;
            int diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var monday = today.AddDays(-diff);

            var list = await _context.AntrenorMusaitlikler
                .Where(m => m.AntrenorId == antrenorId)
                .ToListAsync();

            var events = list.Select(m =>
            {
                var date = monday.AddDays((int)m.Gun - (int)DayOfWeek.Monday);
                var start = date.Date + m.BaslangicSaati;
                var end = date.Date + m.BitisSaati;

                return new
                {
                    id = m.Id,
                    title = "Müsait",
                    start = start.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = end.ToString("yyyy-MM-ddTHH:mm:ss")
                };
            });

            return Json(events);
        }

        // FullCalendar: seçilen aralıktan Müsaitlik ekler
        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] MusaitlikEkleDto dto)
        {
            if (dto == null || dto.AntrenorId == 0)
                return BadRequest();

            var start = dto.Start;
            var end = dto.End;

            if (end <= start)
                return BadRequest("Bitiş başlangıçtan küçük olamaz.");

            var entity = new AntrenorMusaitlik
            {
                AntrenorId = dto.AntrenorId,
                Gun = start.DayOfWeek,
                BaslangicSaati = start.TimeOfDay,
                BitisSaati = end.TimeOfDay
            };

            _context.AntrenorMusaitlikler.Add(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // FullCalendar: event silme
        [HttpPost]
        public async Task<IActionResult> DeleteEvent([FromBody] MusaitlikSilDto dto)
        {
            var entity = await _context.AntrenorMusaitlikler.FindAsync(dto.Id);
            if (entity == null) return NotFound();

            _context.AntrenorMusaitlikler.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
