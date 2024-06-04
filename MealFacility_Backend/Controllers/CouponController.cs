using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using MealFacility_Backend.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

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

        [Authorize]
        [HttpPost("createCoupon")]
        public async Task<IActionResult> CreateCoupon([FromBody] CoupenRequestDto requestDto)
        {
            if (requestDto == null || requestDto.UserId == 0)
            {
                return BadRequest("Invalid request data");
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Id == requestDto.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var createdTime = DateTime.Now;

            var newCoupon = new Coupon
            {
                CouponCode = GenerateRandomAlphanumericCode(10),
                CreatedTime = createdTime,
                ExpirationTime = createdTime.AddMinutes(1),
                UserId = user.Id,
                User = user
            };

            _authContext.Coupons.Add(newCoupon);
            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Coupon created successfully",
                Coupon = newCoupon
            });
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
