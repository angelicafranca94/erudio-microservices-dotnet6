using GeekShopping.ProductAPI.Data.DTO;
using GeekShopping.ProductAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.ProductAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private IProductRepository _repository;

        public ProductController(IProductRepository repository)
        {
            _repository = repository ?? throw new ArgumentException(nameof(repository));
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> FindAll(long id) 
        {
            var produts = await _repository.FindAll();
           return Ok(produts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> FindById(long id)
        {
            var produt = await _repository.FindById(id);
            if (produt == null) return NotFound();
            return Ok(produt);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> Create(ProductDTO productdto)
        {
           
            if (productdto == null) return BadRequest();
            var produt = await _repository.Create(productdto);
            return Ok(produt);
        }

        [HttpPut]
        public async Task<ActionResult<ProductDTO>> Update(ProductDTO productdto)
        {

            if (productdto == null) return BadRequest();
            var produt = await _repository.Update(productdto);
            return Ok(produt);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var status = await _repository.Delete(id);
            if(!status) return BadRequest();
            return Ok(status);
        }
    }
}
