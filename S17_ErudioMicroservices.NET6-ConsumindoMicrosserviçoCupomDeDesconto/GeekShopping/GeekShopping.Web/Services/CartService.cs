using GeekShopping.Web.Models;
using GeekShopping.Web.Utils;
using System.Net.Http.Headers;

namespace GeekShopping.Web.Services;

public class CartService : ICartService
{
    private readonly HttpClient _client;

    public const string BasePath = "api/v1/Cart";

    public CartService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }
    public async Task<CartViewModel> FindCartByUserId(string userId, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"{BasePath}/find-cart/{userId}");
        return await response.ReadContentAs<CartViewModel>();
    }

    public async Task<CartViewModel> AddItemToCart(CartViewModel model, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJson($"{BasePath}/add-cart", model);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartViewModel>();
        else throw new Exception("Alguma coisa deu errado ao chamar a API AddItemToCart");
    }
    public async Task<CartViewModel> UpdateCart(CartViewModel model, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PutAsJson($"{BasePath}/update-cart", model);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<CartViewModel>();
        else throw new Exception("Alguma coisa deu errado ao chamar a API UpdateCart");
    }
    public async Task<bool> RemoveFromCart(long cartId, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync($"{BasePath}/remove-cart/{cartId}");
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        else throw new Exception("Alguma coisa deu errado ao chamar a API RemoveFromCart");
    }


    public async Task<bool> ApplyCoupon(CartViewModel cart, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsJson($"{BasePath}/apply-coupon", cart);
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        else throw new Exception("Alguma coisa deu errado ao chamar a API ApplyCoupon");
    }

    public async Task<bool> RemoveCoupon(string userId, string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync($"{BasePath}/remove-coupon/{userId}");
        if (response.IsSuccessStatusCode)
            return await response.ReadContentAs<bool>();
        else throw new Exception("Alguma coisa deu errado ao chamar a API RemoveCoupon");
    }


    public async Task<CartViewModel> Checkout(CartHeaderViewModel cartHeader, string token)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ClearCart(string userId, string token)
    {
        throw new NotImplementedException();
    }



  



}
