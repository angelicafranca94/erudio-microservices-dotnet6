using AutoMapper;
using GeekShopping.CartAPI.Data.DTO;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GeekShopping.CartAPI.Repository;

public class CouponRepository : ICouponRepository
{

    private readonly HttpClient _client;

    public async Task<CouponDTO> GetCouponByCouponCode(string couponCode, string token)
    {


        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync($"//api/v1/Coupon/{couponCode}");
        var content = await response.Content.ReadAsStringAsync();

        if(response.StatusCode != HttpStatusCode.OK) return new CouponDTO();

        return JsonSerializer.Deserialize<CouponDTO>(content,
           new JsonSerializerOptions
           { PropertyNameCaseInsensitive = true });
    }
}
