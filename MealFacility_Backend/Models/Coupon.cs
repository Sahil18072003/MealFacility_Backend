using System.ComponentModel.DataAnnotations;

namespace MealFacility_Backend.Models
{
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        public string CouponCode { get; set; }

        public DateTime GetDate { get; set; }
    }
}
