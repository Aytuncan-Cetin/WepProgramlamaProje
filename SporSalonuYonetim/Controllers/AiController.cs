using System.Threading.Tasks;
using SporSalonuYonetim.Models;
using SporSalonuYonetim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SporSalonuYonetim.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly GeminiFitnessService _geminiService;

        public AiController(GeminiFitnessService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpGet]
        public IActionResult FitnessRecommendation()
        {
            return View(new FitnessAiRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FitnessRecommendation(FitnessAiRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.ResultText = await _geminiService.GetFitnessPlanAsync(model);

            return View(model);
        }
    }
}
