using GeekShopping.Web.Models;
using GeekShopping.Web.Services;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace GeekShopping.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService,
            AsyncCircuitBreakerPolicy circuitBreaker)
        {
            _logger = logger;
            _productService = productService;
            _cartService = cartService;
            _circuitBreaker = circuitBreaker;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.FindAllProducts(string.Empty);
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                                     $"Circuito = {_circuitBreaker.CircuitState} | " +
                                     $"Falha ao invocar a API: {ex.GetType().FullName} | {ex.Message}");

                HandleBrokenCircuitException();
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var token = await HttpContext.GetTokenAsync("access_token");
                var model = await _productService.FindProductById(id, token);
                return View(model);
            }
            catch (Exception ex)
            {

                _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                                     $"Circuito = {_circuitBreaker.CircuitState} | " +
                                     $"Falha ao invocar a API: {ex.GetType().FullName} | {ex.Message}");

                HandleBrokenCircuitException();
            }
            return View();
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize]
        public async Task<IActionResult> DetailsPost(ProductViewModel model)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            CartViewModel cart = new CartViewModel()
            {
                CartHeader = new CartHeaderViewModel
                {
                    UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
                }
            };

            CartDetailViewModel cartDetail = new CartDetailViewModel()
            {
                Count = model.Count,
                ProductId = model.Id,
                Product = await _productService.FindProductById(model.Id, token)
            };

            List<CartDetailViewModel> cartDetails = new List<CartDetailViewModel>();
            cartDetails.Add(cartDetail);
            cart.CartDetails = cartDetails;

            try
            {
                var response = await _cartService.AddItemToCart(cart, token);
                if (response != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                                     $"Circuito = {_circuitBreaker.CircuitState} | " +
                                     $"Falha ao invocar a API: {ex.GetType().FullName} | {ex.Message}");

                HandleBrokenCircuitException();
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }

        private void HandleBrokenCircuitException()
        {
            TempData["BasketInoperativeMsg"] = "Serviço não disponível, por favor tente mais tarde. (Business message due to Circuit-Breaker)";
        }
    }
}
