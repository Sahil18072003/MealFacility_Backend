using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealFacility_Backend.Models
{
    public class Booking
    {
        public Booking()
        {
            Status = "inactive";
        }
        [Key]
        public int BookingId { get; set; }

        public string BookingType { get; set; }

        public DateTime BookingDate { get; set; }

        public DateTime BookingStartDate { get; set; }

        public DateTime BookingEndDate { get; set; }

        public string? Status { get; set; }

        //public string? CupponId { get; set; }
        //[ForeignKey("CouponId")]
        //public Coupon Coupon { get; set; }

        public int UserId { get; set; }

        [ForeignKey("Id")]
        public User User { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
