using Microsoft.AspNetCore.Mvc;

namespace TEST_MVC.Controllers
{
    public class ProductTypeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
