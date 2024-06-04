using MealFacility_Backend.Models;

namespace MealFacility_Backend.Services.IServices
{
    public interface IEmailServices
    {
        void SendEmail(Email email);
    }
}
