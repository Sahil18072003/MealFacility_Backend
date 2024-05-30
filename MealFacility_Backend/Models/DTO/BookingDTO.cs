using System.ComponentModel.DataAnnotations.Schema;

namespace MealFacility_Backend.Models
{
    public class BookingDto
    {
        public DateTime BookingStartDate { get; set; }

        public DateTime BookingEndDate { get; set; }

        public string? BookingType { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
