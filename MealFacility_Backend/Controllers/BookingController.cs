using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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

            if (bookingObj.BookingStartDate.Date.AddDays(1) <= DateTime.Today)
            {
                return BadRequest("Bookings cannot be made for today or any past dates.");
            }

            if (bookingObj.BookingStartDate.Date.AddDays(1) > DateTime.Today.AddMonths(3))
            {
                return BadRequest("Bookings cannot be made more than 3 months in advance.");
            }

            var holidays = GetHolidays();

            var currentDate = bookingObj.BookingStartDate.Date.AddDays(1);
            var endDate = bookingObj.BookingEndDate.Date.AddDays(1);

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(currentDate))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                var existingBooking = await _authContext.Bookings
                    .Where(b => b.UserId == user.Id && b.BookingDate == currentDate && b.BookingType == bookingObj.BookingType)
                    .FirstOrDefaultAsync();

                if (existingBooking != null)
                {
                    return BadRequest($"A {bookingObj.BookingType} booking already exists for {existingBooking.BookingDate.ToShortDateString()}.");
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
                Message = "Your bookings have been successfully completed."
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

            var bookingDate = bookingObj.BookingDate.Date.AddDays(1);

            if (bookingDate <= DateTime.Today)
            {
                return BadRequest("Bookings cannot be made for today or any past dates.");
            }

            if (bookingDate == DateTime.Today.AddDays(1) && DateTime.Now.Hour >= 20)
            {
                return BadRequest("Bookings for tomorrow cannot be made after 8 PM today.");
            }

            var existingBooking = await _authContext.Bookings
                .Where(b => b.UserId == user.Id && b.BookingDate == bookingDate && b.BookingType == bookingObj.BookingType)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                return BadRequest($"A {bookingObj.BookingType} booking already exists for {existingBooking.BookingDate.ToShortDateString()}.");
            }

            var booking = new Booking
            {
                BookingDate = bookingDate,
                BookingType = bookingObj.BookingType,
                UserId = user.Id,
                Status = "Active",
                TimeStamp = DateTime.Now
            };

            await _authContext.Bookings.AddAsync(booking);

            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Your booking has been successfully completed."
            });
        }

        [Authorize]
        [HttpGet("viewUserBookings")]
        public async Task<IActionResult> ViewBooking([FromQuery] int userId)
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

            var cancelDate = cancelBookingDto.Date.Date.AddDays(1);

            if (cancelBookingDto.BookingType == "both")
            {
                // Fetch both lunch and dinner bookings for the user on the specified date
                var bookings = await _authContext.Bookings
                    .Where(b => b.UserId == user.Id && b.BookingDate.Date == cancelDate && (b.BookingType == "lunch" || b.BookingType == "dinner"))
                    .ToListAsync();

                if (bookings == null || bookings.Count == 0)
                {
                    return NotFound("No bookings found for the selected date.");
                }

                foreach (var booking in bookings)
                {
                    if (DateTime.Now.Date > cancelDate)
                    {
                        return BadRequest("Past bookings cannot be cancelled.");
                    }

                    if (booking.Status == "Cancelled")
                    {
                        return BadRequest($"Your {booking.BookingType} booking for the selected date has already been cancelled.");
                    }

                    booking.Status = "Cancelled";
                    _authContext.Entry(booking).State = EntityState.Modified;
                }

                await _authContext.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Lunch and dinner bookings have been successfully cancelled."
                });
            }
            else
            {
                var booking = await _authContext.Bookings
                    .Where(b => b.UserId == user.Id && b.BookingDate.Date == cancelDate && b.BookingType == cancelBookingDto.BookingType)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    return NotFound($"No {cancelBookingDto.BookingType} booking found for the selected date.");
                }

                if (DateTime.Now.Date > cancelDate)
                {
                    return BadRequest("Past bookings cannot be cancelled.");
                }

                if (booking.Status == "Cancelled")
                {
                    return BadRequest($"Your {cancelBookingDto.BookingType} booking for the selected date has already been cancelled.");
                }

                booking.Status = "Cancelled";
                _authContext.Entry(booking).State = EntityState.Modified;

                await _authContext.SaveChangesAsync();

                return Ok(new
                {
                    Message = $"{cancelBookingDto.BookingType} booking has been successfully cancelled."
                });
            }
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
