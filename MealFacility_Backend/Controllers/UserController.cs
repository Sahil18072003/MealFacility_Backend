using MealFacility_Backend.Context;
using MealFacility_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MealFacility_Backend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MealFacility_Backend.UtilityServices;
using MealFacility_Backend.Models.DTO;
using static System.Net.WebRequestMethods;

namespace MealFacility_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _authContext;

        private readonly IConfiguration _configration;

        private readonly IEmailServices _emailService;

        public UserController(AppDbContext appDbContext, IConfiguration configuration, IEmailServices emailServices)
        {
            _authContext = appDbContext;
            _configration = configuration;
            _emailService = emailServices;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.email == userObj.email);

            if (user == null)
                return NotFound(new { message = "User Not Found!" });

            if (!PasswordHasher.VerifyPassword(userObj.password, user.password))
            {
                return BadRequest(new { message = "Password is Incorrect" });
            }

            user.Token = CreateJwt(user);

            return Ok(new
            {
                user = user,
                Message = "Login Success!"
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest(new { message = "Invalid request. User data is missing." });

            // Check email
            if (await CheckEmailExistAsync(userObj.email))
                return BadRequest(new { message = "Email address is already in use." });

            // Check userName
            if (await CheckUserNameExistAsync(userObj.userName))
                return BadRequest(new { message = "Username is already taken." });

            // Check password Strength
            var pass = CheckPasswordStrength(userObj.password);
            if (!string.IsNullOrEmpty(pass))
                return BadRequest(new { message = pass });

            userObj.password = PasswordHasher.HashPassword(userObj.password);
            userObj.Token = "";

            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully."
            });
        }

        private Task<bool> CheckEmailExistAsync(string email)
            => _authContext.Users.AnyAsync(x => x.email == email);

        private Task<bool> CheckUserNameExistAsync(string userName)
            => _authContext.Users.AnyAsync(x => x.userName == userName);

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if (password.Length < 8)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);

            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should be Alphanumeric" + Environment.NewLine);

            if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password should contain special chars" + Environment.NewLine);
            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes("veryveryveryveryveryveryverysceret....");

            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Name,$"{user.firstName} {user.lastName}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }


        //[Authorize]
       /* [HttpGet("getUser/{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        } 


        //[Authorize]
        [HttpGet("getAllUsers")]
        public async Task<ActionResult<User>> GetAllUser()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }
        */

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> SendEmail([FromBody] User userObj)
        {
            var email = userObj.email;

            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.email == email);

            if (user is null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User doesn't exist"
                });
            }

            // Generate a 6-digit OTP
            string Otp = GenerateOTP();

            user.GeneratedOTP = Otp;
            user.TimeStamp = DateTime.UtcNow;

            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();

            string from = _configration["EmailSettings:From"];

            var emailModel = new Email(email, "OTP for Reset Password!!", EmailBody.EmailStringBody(email, Otp));

            _emailService.SendEmail(emailModel);

            return Ok(new
            {
                StatusCode = 200,
                Message = "Otp sent successfully on given email",
                OTP = Otp
            });
        }

        private string GenerateOTP()
        {
            Random rnd = new Random();
            int otpNumber = rnd.Next(100000, 999999); // Generates a random number between 100000 and 999999
            return otpNumber.ToString();
        }

        [HttpPost("verifyOTP")]
        public IActionResult VerifyOTP([FromBody] OTPVerificationDto otpVerificationDto)
        {
            // Retrieve user by email
            var user = _authContext.Users.FirstOrDefault(x => x.email == otpVerificationDto.email);

            if (user == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User not found"
                });
            }

            // Check if OTP matches
            if (otpVerificationDto.EnteredOTP != user.GeneratedOTP)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Entered OTP is incorrect"
                });
            }

            // OTP is correct, perform further actions if needed

            return Ok(new
            {
                StatusCode = 200,
                Message = "OTP verification successful"
            });
        }


        [HttpPost("resetPassword")]
        public async Task<IActionResult> ConfirmPassword([FromBody] NewPasswordDto newPasswordDto)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.email == newPasswordDto.Email);

            if (user is null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User doesn't exist"
                });
            }

            user.password = PasswordHasher.HashPassword(newPasswordDto.Password);

            _authContext.Update(user); // Update the existing user

            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Password reset successfully",
            });
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.email == changePasswordDto.Email);

            if (user is null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "User doesn't exist"
                });
            }

            // Check if entered old password matches the current password
            if (!PasswordHasher.VerifyPassword(changePasswordDto.Password, user.password))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Old password is incorrect."
                });
            }

            // Check if Password and NewPassword are the same
            if (changePasswordDto.Password == changePasswordDto.NewPassword)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "New password must be different from the current password"
                });
            }

            // Hash the new password
            user.password = PasswordHasher.HashPassword(changePasswordDto.NewPassword);

            _authContext.Update(user); // Update the existing user

            await _authContext.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Password changed successfully",
            });
        }
    }
}
