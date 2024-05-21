using MealFacility_Backend.Models;

namespace MealFacility_Backend.UtilityServices
{
    public interface IEmailServices
    {
        void SendEmail(Email email);
    }
}
