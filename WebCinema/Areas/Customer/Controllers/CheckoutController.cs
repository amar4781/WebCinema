using Microsoft.AspNetCore.Mvc;

namespace WebCinema.Areas.Customer.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Success()
        {
            // 1. Create New Order
            // 2. Create Order Items
            // 3. Remove Old Cart
            // 4. Update Quantity in Stock

            return Ok();
        }

        public IActionResult Cancel()
        {
            return Ok();
        }
    }
}
