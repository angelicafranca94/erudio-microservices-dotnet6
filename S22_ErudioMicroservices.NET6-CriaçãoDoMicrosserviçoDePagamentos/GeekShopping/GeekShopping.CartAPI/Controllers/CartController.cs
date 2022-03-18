using GeekShopping.CartAPI.Data.DTO;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CartController : ControllerBase
    {

        private ICartRepository _cartrepository;
        private ICouponRepository _couponrepository;
        private IRabbitMQMessageSender _rabbitMQMessageSender;

        public CartController(ICartRepository cartrepository, ICouponRepository couponrepository, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _cartrepository = cartrepository ?? throw new ArgumentNullException(nameof(cartrepository));
            _couponrepository = couponrepository ?? throw new ArgumentNullException(nameof(couponrepository));
            _rabbitMQMessageSender = rabbitMQMessageSender ?? throw new ArgumentNullException(nameof(rabbitMQMessageSender));
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartDTO>> FindById(string id)
        {
            var cart = await _cartrepository.FindCartByUserId(id);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

        [HttpPost("add-cart")]
        public async Task<ActionResult<CartDTO>> AddCart(CartDTO cartDTO)
        {
            var cart = await _cartrepository.SaveOrUpdateCart(cartDTO);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

        [HttpPut("update-cart")]
        public async Task<ActionResult<CartDTO>> UpdateCart(CartDTO cartDTO)
        {
            var cart = await _cartrepository.SaveOrUpdateCart(cartDTO);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<CartDTO>> RemoveCart(int id)
        {
            var status = await _cartrepository.RemoveFromCart(id);
            if (!status) return NotFound();
            return Ok(status);
        }


        [HttpPost("apply-coupon")]
        public async Task<ActionResult<CartDTO>> ApplyCoupon(CartDTO cartDTO)
        {
            var status = await _cartrepository.ApplyCoupon(cartDTO.CartHeader.UserId, cartDTO.CartHeader.CouponCode);
            if (!status) return NotFound();
            return Ok(status);
        }

        [HttpDelete("remove-coupon/{userId}")]
        public async Task<ActionResult<CartDTO>> RemoveCoupon(string userId)
        {
            var status = await _cartrepository.RemoveCoupon(userId);
            if (!status) return NotFound();
            return Ok(status);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutHeaderDTO>> Checkout(CheckoutHeaderDTO dto)
        {
            string token = Request.Headers["Authorization"];
            if (dto?.UserId == null) return BadRequest();
            var cart = await _cartrepository.FindCartByUserId(dto.UserId);
            if (cart == null) return NotFound();
            if(!string.IsNullOrEmpty(dto.CouponCode))
            {
                CouponDTO coupon = await _couponrepository.GetCoupon(dto.CouponCode, token);
                if (dto.DiscountAmount != coupon.DiscountAmount)
                {
                    return StatusCode(412);
                }
            }

            dto.CartDetails = cart.CartDetails;
            dto.DateTime = DateTime.Now;

            //TASK RabbitMQ logic comes here!!!

            _rabbitMQMessageSender.SendMessage(dto, "checkoutqueue");

            await _cartrepository.ClearCart(dto.UserId);

            return Ok(dto);
        }
    }
}