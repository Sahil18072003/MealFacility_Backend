namespace MealFacility_Backend.Models.DTO
{
    public class CancelBookingDto
    {
        public int UserId { get; set; }

        public DateTime Date { get; set; }

        public string? BookingType { get; set; }
    }
}
