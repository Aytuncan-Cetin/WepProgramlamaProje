using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;
using SporSalonuYonetim.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SporSalonuYonetim.Controllers
{
    [Authorize]
    public class YapayZekaController : Controller
    {
        private readonly GeminiAiService _gemini;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UygulamaKullanicisi> _userManager;

        public YapayZekaController(
            GeminiAiService gemini,
            ApplicationDbContext context,
            UserManager<UygulamaKullanicisi> userManager)
        {
            _gemini = gemini;
            _context = context;
            _userManager = userManager;
        }

        // Form ekranı
        public IActionResult Index()
        {
            return View(new YapayZekaOneri());
        }

        // Form post edildiğinde
        [HttpPost]
        public async Task<IActionResult> Index(YapayZekaOneri model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);

            string sonucMetni = await _gemini.EgzersizOnerisiOlustur(
                model.Boy,
                model.Kilo,
                model.Hedef,
                model.VucutTipi
            );

            // DB'ye kayıt
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var kayit = new YapayZekaOneriKaydi
                {
                    UyeId = userId,
                    Boy = model.Boy,
                    Kilo = model.Kilo,
                    Hedef = model.Hedef,
                    VucutTipi = model.VucutTipi,
                    OneriMetni = sonucMetni
                };

                _context.YapayZekaOneriKayitlari.Add(kayit);
                await _context.SaveChangesAsync();
            }

            var sonuc = new YapayZekaSonuc
            {
                OneriMetni = sonucMetni
            };

            return View("Sonuc", sonuc);
        }

        // Kullanıcının geçmiş AI önerileri
        public async Task<IActionResult> Gecmis()
        {
            var userId = _userManager.GetUserId(User);

            var liste = await _context.YapayZekaOneriKayitlari
                .Where(x => x.UyeId == userId)
                .OrderByDescending(x => x.OlusturulmaTarihi)
                .ToListAsync();

            return View(liste);
        }
    }
}
