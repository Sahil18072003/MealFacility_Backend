using System.ComponentModel.DataAnnotations;

namespace MealFacility_Backend.Models.DTO
{
    public class ChangePasswordDto
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string NewPassword { get; set; }
    }
}
