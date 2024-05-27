using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealFacility_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _notificationContext;

        public NotificationController(AppDbContext context)
        {
            _notificationContext = context;
        }

        [HttpPost("notification")]
        public async Task<ActionResult<Notification>> CreateNotification([FromBody] Notification notification)
        {
            if (notification == null)
                return BadRequest("Invalid notification data.");

            notification.TimeStamp = DateTime.Now;
            _notificationContext.Notifications.Add(notification);
            await _notificationContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotificationById), new { id = notification.Id }, notification);
        }

        [HttpGet("getAllNotifications")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetAllNotifications()
        {
            return Ok(await _notificationContext.Notifications.ToListAsync());
        }

        [HttpGet("getNotification/{id}")]
        public async Task<ActionResult<Notification>> GetNotificationById(int id)
        {
            var notification = await _notificationContext.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            return Ok(notification);
        }

        [HttpDelete("deleteNotification/{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _notificationContext.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            _notificationContext.Notifications.Remove(notification);
            await _notificationContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("deleteAllNotifications")]
        public IActionResult DeleteAllNotifications()
        {
            _notificationContext.Notifications.RemoveRange(_notificationContext.Notifications);
            _notificationContext.SaveChanges();

            return Ok();
        }
    }
}
