using Microsoft.AspNetCore.Mvc;

namespace WebCinema.Areas.Customer.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Refund()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Refund(int orderId)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Review()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Review(ReviewVM reviewVM)
        {
            return View();
        }
    }
}
