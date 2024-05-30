using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MealFacility_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _authContext;

        public BookingController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [Authorize]
        [HttpPost("bulkBooking")]
        public async Task<IActionResult> BulkBooking([FromBody] BookingDto bookingObj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Id == bookingObj.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (bookingObj.BookingStartDate.Date <= DateTime.Today)
            {
                return BadRequest("Booking for today or any past date is not allowed.");
            }

            if (bookingObj.BookingStartDate.Date > DateTime.Today.AddMonths(3))
            {
                return BadRequest("Booking cannot be made more than 3 months in advance.");
            }

            var holidays = GetHolidays(); // Assume this method returns a list of holiday dates
            var currentDate = bookingObj.BookingStartDate;

            while (currentDate <= bookingObj.BookingEndDate)
            {
                // Skip weekends and holidays
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(currentDate))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // Check for overlapping bookings
                var existingBooking = await _authContext.Bookings
                    .Where(b => b.UserId == user.Id && b.BookingDate == currentDate)
                    .FirstOrDefaultAsync();

                if (existingBooking != null)
                {
                    return BadRequest($"You already have a booking on {existingBooking.BookingDate.ToShortDateString()}.");
                }

                var booking = new Booking
                {
                    BookingDate = currentDate,
                    BookingType = bookingObj.BookingType,
                    UserId = user.Id,
                    Status = "Active",
                    TimeStamp = DateTime.Now
                };

                await _authContext.Bookings.AddAsync(booking);

                // Move to the next day
                currentDate = currentDate.AddDays(1);
            }

            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Your booking was successfully completed!"
            });
        }

        private List<DateTime> GetHolidays()
        {
            // Return a list of holidays
            return new List<DateTime>
            {
                new DateTime(DateTime.Today.Year, 12, 25), // Example: Christmas
                // Add more holidays as needed
            };
        }

        [Authorize]
        [HttpPost("quickBooking")]
        public async Task<IActionResult> QuickBooking([FromBody] Booking bookingObj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Id == bookingObj.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (bookingObj.BookingDate.Date <= DateTime.Today)
            {
                return BadRequest("Booking for today or any past date is not allowed.");
            }

            // Ensure the booking button is visible until 8 PM the previous day
            if (bookingObj.BookingDate.Date == DateTime.Today.AddDays(1) && DateTime.Now.Hour >= 20)
            {
                return BadRequest("Booking for tomorrow is not allowed after 8 PM today.");
            }

            // Check for overlapping bookings
            var existingBooking = await _authContext.Bookings
                .Where(b => b.UserId == user.Id && b.BookingDate == bookingObj.BookingDate)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                return BadRequest($"You already have a booking on {existingBooking.BookingDate.ToShortDateString()}.");
            }

            var booking = new Booking
            {
                BookingDate = bookingObj.BookingDate,
                BookingType = bookingObj.BookingType,
                UserId = user.Id,
                Status = "Active",
                TimeStamp = DateTime.Now
            };

            await _authContext.Bookings.AddAsync(booking);
            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Your booking was successfully completed!"
            });
        }

        [Authorize]
        [HttpGet("getUserBookings/{userId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByUserId(int userId)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var bookings = await _authContext.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();

            if (bookings == null || bookings.Count == 0)
            {
                return NotFound("No bookings found for this user.");
            }

            return Ok(bookings);
        }

        [Authorize]
        [HttpDelete("cancelBooking/{date}")]
        public async Task<IActionResult> CancelBooking(DateTime date)
        {
            var username = User?.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.userName == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var booking = await _authContext.Bookings
                .Where(b => b.User.userName == username && b.BookingDate.Date == date.Date)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound("Booking not found for the selected date.");
            }

            if (DateTime.Now.Date == date.Date && DateTime.Now.Hour >= 20)
            {
                return BadRequest("Cannot cancel booking on the same day after 8 PM.");
            }

            if (DateTime.Now.Date > date.Date)
            {
                return BadRequest("Cannot cancel past bookings.");
            }

            // Remove the booking
            _authContext.Bookings.Remove(booking);
            await _authContext.SaveChangesAsync();

            // Automatically add a booking for the next valid day
            var lastBookingDate = await _authContext.Bookings
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => b.BookingDate)
                .FirstOrDefaultAsync();

            var newBookingDate = GetNextValidDate(lastBookingDate.AddDays(1));
            var newBooking = new Booking
            {
                BookingDate = newBookingDate,
                BookingType = booking.BookingType,
                UserId = user.Id,
                Status = "Active",
                TimeStamp = DateTime.Now
            };
            await _authContext.Bookings.AddAsync(newBooking);

            await _authContext.SaveChangesAsync();

            return Ok("Booking cancelled successfully.");
        }

        private DateTime GetNextValidDate(DateTime startDate)
        {
            var newDate = startDate;
            while (newDate.DayOfWeek == DayOfWeek.Saturday || newDate.DayOfWeek == DayOfWeek.Sunday)
            {
                newDate = newDate.AddDays(1);
            }
            return newDate;
        }
    }
}
