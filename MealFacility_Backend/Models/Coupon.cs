using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealFacility_Backend.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        public string? CouponCode { get; set; }

        public DateTime? CreatedTime { get; set; }

        public int UserId { get; set; }

        [ForeignKey("Id")]
        public User? User { get; set; }

        public DateTime? ExpirationTime { get; set; }
    }
}
