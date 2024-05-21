using System.ComponentModel.DataAnnotations.Schema;

namespace MealFacility_Backend.Models.DTO
{
    public class BookingDTO
    {
        public BookingDTO()
        {
            Status = "inactive";
        }
        public int BookingId { get; set; }

        public string BookingType { get; set; }

        public DateTime BookingDate { get; set; }

        public DateTime BookingStartDate { get; set; }

        public DateTime BookingEndDate { get; set; }

        public string? Status { get; set; }
    }
}
