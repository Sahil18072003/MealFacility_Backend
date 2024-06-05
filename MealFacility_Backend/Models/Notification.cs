using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealFacility_Backend.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string? Message { get; set; }

        public int UserId { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}
