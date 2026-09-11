using Microsoft.AspNetCore.Mvc;

namespace KnjiznicaAPI.Controllers
{
    public class BookAuthorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
