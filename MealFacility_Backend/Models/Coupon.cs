using System.ComponentModel.DataAnnotations;

namespace MealFacility_Backend.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        public string? CouponCode { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? ExpirationTime { get; set; }
    }
}
