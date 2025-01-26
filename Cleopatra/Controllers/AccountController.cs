using Cleopatra.Data;
using Cleopatra.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Cleopatra.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        // POST: api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (model.Password != model.ConfirmPassword)
                return BadRequest("Passwords don't match");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Add user to the Customers table
            var customer = new Customer
            {
                IdentityUserId = user.Id,
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                CreatedDate = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                Message = "User registered successfully."
            });
        }

        // POST: api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { Message = "Invalid email or password." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { Message = "Invalid email or password." });

            // Generate JWT token
            var token = GenerateJwtToken(user);

            string role = _userManager.GetRolesAsync(user).Result.FirstOrDefault() == "Admin" ? "Admin" : "User";

            if (role == "User")
            {
                return Ok(new
                {
                    Token = token,
                    UserId = _context.Customers.Where(c => c.IdentityUserId == user.Id).Select(c => c.CustomerId).FirstOrDefault(),
                    Role = role,
                    Message = "Login successful."
                });
            }
            
            if (role == "Admin")
            {
                return Ok(new
                {
                    Token = token,
                    UserId = _context.Employees.Where(c => c.IdentityUserId == user.Id).Select(c => c.EmployeeId).FirstOrDefault(),
                    Role = role,
                    Message = "Login successful."
                });
            }
            
            return BadRequest(new { Message = "Error occured while determining the role of a user" });
        }

        // POST: api/account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logout successful." });
        }

        // JWT token generator
        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
