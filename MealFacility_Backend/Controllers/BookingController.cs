using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using MealFacility_Backend.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        //[Authorize]
        [HttpPost("booking")]
        public async Task<IActionResult> AddBooking([FromBody] BookingDTO bookingObj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User ID not found in token.");
            }

            if (bookingObj.BookingStartDate.Date <= DateTime.Today)
            {
                return BadRequest("Booking for today or any past date is not allowed.");
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.userName == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check for overlapping bookings
            var existingBooking = await _authContext.Bookings
                .Where(b => b.BookingEndDate >= bookingObj.BookingStartDate &&
                            b.BookingStartDate <= bookingObj.BookingEndDate)
                .FirstOrDefaultAsync();

            if (existingBooking != null)
            {
                return BadRequest($"You already have a booking from {existingBooking.BookingStartDate.ToShortDateString()} " +
                    $"to {existingBooking.BookingEndDate.ToShortDateString()}.");
            }

            var booking = new Booking
            {
                BookingDate = DateTime.Now,
                BookingType = bookingObj.BookingType,
                BookingStartDate = bookingObj.BookingStartDate,
                BookingEndDate = bookingObj.BookingEndDate,
            };

            await _authContext.Bookings.AddAsync(booking);
            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                Booking = booking,
                Message = "Your Booking successfully Done!"
            });
        }

        [Authorize]
        [HttpGet("getBooking/{id}")]
        public async Task<ActionResult<Booking>> GetUserById(int id)
        {
            var booking = await _authContext.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }


        [Authorize]
        [HttpDelete("cancelBooking/{date}")]
        public async Task<IActionResult> CancelBooking(DateTime date)
        {
            var username = User.Identity.Name;

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
                .Where(b => b.User.userName == username &&
                            b.BookingStartDate <= date &&
                            b.BookingEndDate >= date)
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

            if (booking.BookingStartDate == booking.BookingEndDate)
            {
                _authContext.Bookings.Remove(booking);
            }
            else if (booking.BookingStartDate == date)
            {
                booking.BookingStartDate = date.AddDays(1);
            }
            else if (booking.BookingEndDate == date)
            {
                booking.BookingEndDate = date.AddDays(-1);
            }
            else
            {
                var newBooking = new Booking
                {
                    BookingDate = booking.BookingDate,
                    BookingType = booking.BookingType,
                    BookingStartDate = date.AddDays(1),
                    BookingEndDate = booking.BookingEndDate,
                    User = booking.User
                };

                booking.BookingEndDate = date.AddDays(-1);

                _authContext.Bookings.Add(newBooking);
            }

            await _authContext.SaveChangesAsync();

            return Ok("Booking cancelled successfully.");
        }

    }
}
