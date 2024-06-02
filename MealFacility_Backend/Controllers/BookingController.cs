using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using MealFacility_Backend.Models.DTO;

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

            var holidays = GetHolidays();
            var currentDate = bookingObj.BookingStartDate;

            while (currentDate <= bookingObj.BookingEndDate)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(currentDate))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

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
            return new List<DateTime>
            {
                new DateTime(DateTime.Today.Year, 12, 25),
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

            if (bookingObj.BookingDate.Date == DateTime.Today.AddDays(1) && DateTime.Now.Hour >= 20)
            {
                return BadRequest("Booking for tomorrow is not allowed after 8 PM today.");
            }

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
        [HttpGet("viewUserBookings/{userId}")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetBookingsByUserId(int userId)
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
        [HttpPut("cancelBooking")]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingDto cancelBookingDto)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Id == cancelBookingDto.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var booking = await _authContext.Bookings
                .Where(b => b.UserId == user.Id && b.BookingDate.Date == cancelBookingDto.Date)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound("Booking not found for the selected date.");
            }

            if (DateTime.Now.Date > cancelBookingDto.Date)
            {
                return BadRequest("Cannot cancel past bookings.");
            }

            booking.Status = "Cancelled";
            _authContext.Entry(booking).State = EntityState.Modified;

            await _authContext.SaveChangesAsync();

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

            return Ok(new
            {
                Message = "Booking cancelled successfully."
            });
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