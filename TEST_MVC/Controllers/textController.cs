using Microsoft.AspNetCore.Mvc;

namespace TEST_MVC.Controllers
{
    public class textController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}