using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace GeekShopping.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        public CartController(IProductService productService, ICartService cartService, ICouponService couponService,
             ILogger<CartController> logger, AsyncCircuitBreakerPolicy circuitBreaker)
        {
            _productService = productService;
            _cartService = cartService;
            _couponService = couponService;
            _logger = logger;
            _circuitBreaker = circuitBreaker;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await FindUserCart());
        }

        [HttpPost]
        [ActionName("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartViewModel model)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;

            try
            {
                var response = await _cartService.ApplyCoupon(model, token);

                if (response)
                {
                    return RedirectToAction(nameof(CartIndex));
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

        [HttpPost]
        [ActionName("RemoveCoupon")]
        public async Task<IActionResult> RemoveCoupon()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;

            var response = await _cartService.RemoveCoupon(userId, token);

            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }

        public async Task<IActionResult> Remove(int id)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;

            var response = await _cartService.RemoveFromCart(id, token);

            if (response)
            {
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            return View(await FindUserCart());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CartViewModel model)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            try
            {
                var response = await _cartService.Checkout(model.CartHeader, token);

                if (response != null && response.GetType() == typeof(string))
                {
                    TempData["Error"] = response;
                    return RedirectToAction(nameof(Checkout));
                }
                else if (response != null)
                {
                    return RedirectToAction(nameof(Confirmation));
                }
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

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {
            return View();
        }

        private async Task<CartViewModel> FindUserCart()
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;

            try
            {
                var response = await _cartService.FindCartByUserId(userId, token);

                if (response?.CartHeader != null)
                {
                    if (!string.IsNullOrEmpty(response.CartHeader.CouponCode))
                    {
                        var coupon = await _couponService.GetCoupon(response.CartHeader.CouponCode, token);

                        //se cupom E cuponcode
                        if (coupon?.CouponCode != null)
                        {
                            response.CartHeader.DiscountAmount = coupon.DiscountAmount;

                            response.CartHeader.PurchaseAmount -= response.CartHeader.DiscountAmount;


                        }
                        else
                        {
                            _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                                                     $"Circuito = {_circuitBreaker.CircuitState} | " +
                                                     $"Falha ao invocar a API: CouponApi");

                            HandleBrokenCircuitException();

                            response.CartHeader.CouponCode = string.Empty;


                        }
                    }

                    foreach (var detail in response.CartDetails)
                    {
                        response.CartHeader.PurchaseAmount += (detail.Product.Price * detail.Count);
                    }

                }

                return response;
            }
            catch (Exception ex)
            {

                _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                                     $"Circuito = {_circuitBreaker.CircuitState} | " +
                                     $"Falha ao invocar a API: {ex.GetType().FullName} | {ex.Message}");

                HandleBrokenCircuitException();
            }

            return null;
        }

        private void HandleBrokenCircuitException()
        {
            TempData["BasketInoperativeMsg"] = "Serviço não disponível, por favor tente mais tarde. (Business message due to Circuit-Breaker)";
        }
    }
}
