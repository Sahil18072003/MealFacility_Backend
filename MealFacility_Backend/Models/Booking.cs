using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealFacility_Backend.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string? BookingType { get; set; }

        public DateTime BookingDate { get; set; }

        public string? Status { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
