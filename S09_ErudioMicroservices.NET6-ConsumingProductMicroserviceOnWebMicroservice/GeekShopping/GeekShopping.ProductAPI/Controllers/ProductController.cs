using GeekShopping.ProductAPI.Data.DTO;
using GeekShopping.ProductAPI.Repository;
using GeekShopping.ProductAPI.Utils;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> FindAll(long id) 
        {
            var produts = await _repository.FindAll();
           return Ok(produts);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductDTO>> FindById(long id)
        {
            var produt = await _repository.FindById(id);
            if (produt == null) return NotFound();
            return Ok(produt);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductDTO>> Create([FromBody]ProductDTO productdto)
        {
           
            if (productdto == null) return BadRequest();
            var produt = await _repository.Create(productdto);
            return Ok(produt);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<ProductDTO>> Update([FromBody] ProductDTO productdto)
        {

            if (productdto == null) return BadRequest();
            var produt = await _repository.Update(productdto);
            return Ok(produt);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<ActionResult> Delete(long id)
        {
            var status = await _repository.Delete(id);
            if(!status) return BadRequest();
            return Ok(status);
        }
    }
}
