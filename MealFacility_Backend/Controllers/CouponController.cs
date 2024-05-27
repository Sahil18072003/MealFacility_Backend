using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace MealFacility_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly AppDbContext _authContext;

        public CouponController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [HttpPost("coupon")]
        public async Task<IActionResult> GetCoupon([FromBody] Coupon coupon)
        {
            var createdTime = DateTime.Now;

            var newcoupon = new Coupon
            {
                CouponCode = GenerateRandomAlphanumericCode(10),
                CreatedTime = createdTime,
                ExpirationTime = createdTime.AddMinutes(1),
            };


            _authContext.Coupons.Add(newcoupon);
            await _authContext.SaveChangesAsync();

            return Ok(newcoupon);
        }

        private static string GenerateRandomAlphanumericCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {

                result.Append(chars[random.Next(chars.Length)]);
            }
            return result.ToString();
        }
    }
}
