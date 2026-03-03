using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using WebCinema.Repositories;

namespace WebCinema.Areas.Customer.Controllers
{
    [Area(SD.CUSTOMER_AREA)]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly ILogger<CartController> _logger;

        public CartController(UserManager<ApplicationUser> userManager,
            IRepository<Cart> cartRepository,
            IRepository<Movie> movieRepository,
            IRepository<Promotion> promotionRepository,
            ILogger<CartController> logger)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _movieRepository = movieRepository;
            _promotionRepository = promotionRepository;
            _logger = logger;
        }

        public async Task<IActionResult> AddToCart(int movieId, int count)
        {
            var user = await _userManager.GetUserAsync(User);
            var movie = await _movieRepository.GetOneAsync(e => e.Id == movieId);

            if (user is null || movie is null) return NotFound();

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == movieId);

            if (cartInDb is null)
            {
                await _cartRepository.CreateAsync(new Cart()
                {
                    ApplicationUserId = user.Id,
                    MovieId = movieId,
                    Count = count,
                    listPrice = movie.Price
                });
            }
            else
                cartInDb.Count += count;

            await _cartRepository.CommitAsync();
            TempData["success-notification"] = "Movie added to cart successfully!";

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Index(string? code = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var carts = await _cartRepository.GetAsync(e=>e.ApplicationUserId == user.Id, includes: [e=>e.Movie]);

            if (code is not null)
            {
                var promotion = (await _promotionRepository.GetAsync(e => e.Code == code)).FirstOrDefault();

                if (promotion is null)
                {
                    TempData["error-notification"] = "Invalid promotion code!";
                    return View(carts);
                }

                var cartItem = carts.FirstOrDefault(e => e.MovieId == promotion.MovieId);

                if (cartItem is not null)
                {
                    var discount = promotion.Discount;

                    var totalPrice = cartItem.Movie.Price * cartItem.Count;

                    var discountedTotal = totalPrice - (totalPrice * (discount / 100m));

                    cartItem.listPrice = discountedTotal;

                    await _cartRepository.CommitAsync();

                    TempData["success-notification"] = $"Promotion {code} applied successfully!";
                }
                else
                {
                    TempData["error-notification"] = $"Promotion {code} is not applicable for any movie in your cart!";
                }
            }

            return View(carts);
        }

        public async Task<IActionResult> Increment(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == movieId && e.ApplicationUserId == user.Id, includes: [e=>e.Movie]);
            if (cart == null) return NotFound();

            if (cart.Count < cart.Movie.Quantity)
            {
                cart.Count += 1;

                cart.listPrice = cart.Movie.Price * cart.Count;

                await _cartRepository.CommitAsync();
            }
            else
            {
                TempData["warning-notification"] = "You have reached the maximum quantity for this movie!";
            }


            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Decrement(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == movieId && e.ApplicationUserId == user.Id, includes: [e => e.Movie]);
            if (cart == null) return NotFound();

            if (cart.Count > 1)
            {
                cart.Count -= 1;
                cart.listPrice = cart.Movie.Price * cart.Count;
                await _cartRepository.CommitAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var cart = await _cartRepository.GetOneAsync(e => e.MovieId == movieId && e.ApplicationUserId == user.Id);
            if (cart == null) return NotFound();

            _cartRepository.Delete(cart);
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/cancel",
            };

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e => e.Movie]);
            foreach (var item in carts)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "USD",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Name,
                            Description = item.Movie.Description,
                        },
                        UnitAmount = (long)item.listPrice * 100,
                    },
                    Quantity = item.Count,
                });
            }
            

            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }
    }
}
