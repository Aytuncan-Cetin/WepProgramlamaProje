using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SporSalonuYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RaporlarController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
