using MealFacility_Backend.Models;

namespace Backend.Backend.Service.IUtilityService
{
    public interface INotificationService
    {
        Task<Notification> CreateNotification(Notification notification);
        Task<IEnumerable<Notification>> GetAllNotifications();
        Task<List<Notification>> GetNotificationById(int id);
        Task DeleteNotification(int id);
        Task DeleteAllNotifications();
    }
}
