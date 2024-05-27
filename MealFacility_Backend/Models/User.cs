using System.ComponentModel.DataAnnotations;

namespace MealFacility_Backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? userName { get; set; }
        public string email { get; set; }
        public string? password { get; set; }
        public string? Token { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
