using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace GeekShopping.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

        public ProductController(IProductService productService, ILogger<ProductController> logger, AsyncCircuitBreakerPolicy circuitBreaker)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger;
            _circuitBreaker = circuitBreaker;
        }

        public async Task<IActionResult> ProductIndex()
        {

            try
            {
                var products = await _productService.FindAllProducts(string.Empty);
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Falha ao invocar a API: {ex.GetType().FullName}", DateTime.UtcNow);

                _logger.LogError($"# {DateTime.Now:HH:mm:ss} # " +
                                     $"Circuito = {_circuitBreaker.CircuitState} | " +
                                     $"Falha ao invocar a API: {ex.GetType().FullName} | {ex.Message}");

                HandleBrokenCircuitException();
            }
            return View();
        }

        public async Task<IActionResult> ProductCreate()
        {

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductViewModel model)
        {

            if (ModelState.IsValid)
            {
                var token = await HttpContext.GetTokenAsync("access_token");
                var response = await _productService.CreateProduct(model, token);
                if (response != null) return RedirectToAction(nameof(ProductIndex));
            }

            return View(model);

        }

        public async Task<IActionResult> ProductUpdate(int id)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var model = await _productService.FindProductById(id, token);
            if (model != null) return View(model);
            return NotFound();

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProductUpdate(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var token = await HttpContext.GetTokenAsync("access_token");
                var response = await _productService.UpdateProduct(model, token);
                if (response != null) return RedirectToAction(nameof(ProductIndex));
            }

            return View(model);

        }

        [Authorize]
        public async Task<IActionResult> ProductDelete(int id)
        {

            var token = await HttpContext.GetTokenAsync("access_token");
            var model = await _productService.FindProductById(id, token);
            if (model != null) return View(model);
            return NotFound();

            return View();
        }


        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> ProductDelete(ProductViewModel model)
        {

            var token = await HttpContext.GetTokenAsync("access_token");
            var response = await _productService.DeleteProductById(model.Id, token);
            if (response) return RedirectToAction(nameof(ProductIndex));

            return View(model);

        }

        private void HandleBrokenCircuitException()
        {
            TempData["BasketInoperativeMsg"] = "Serviço não disponível, por favor tente mais tarde. (Business message due to Circuit-Breaker)";
        }
    }
}
