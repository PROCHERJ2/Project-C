﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;    //new
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;




namespace AttendifyServerProjectC.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly PasswordHasher<IdentityUser> _passwordHasher;    //new account creation

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthenticationController(
            ApplicationDbContext context, 
            ILogger<AuthenticationController> logger, 
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager, 
            IConfiguration config,
            IOptions<JwtBearerOptions> jwtBearerOptions,
            IOptionsMonitor<JwtBearerOptions> jwtBearerOptionsMonitor)
        {
            _context = context;                                                                                                         //for database access
            _logger = logger;                                                                                                           //for logs
            _passwordHasher = new PasswordHasher<IdentityUser>();                                                                       //new account creation
            _userManager = userManager;                                                                                                 //for login
            _signInManager = signInManager;                                                                                             //for login
            _tokenValidationParameters = jwtBearerOptionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters; //for login
            _config = config;
        }

        //-----------------------------------------------------------login--------------------------------------------------------

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AttendifySharedProjectC.Models.LoginModel model)
        {
            _logger.LogInformation("SecretKey: {SecretKey}", _config["Jwt:SecretKey"]);
            _logger.LogInformation("Issuer: {Issuer}", _config["Jwt:Issuer"]);
            _logger.LogInformation("Audience: {Audience}", _config["Jwt:Audience"]);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Invalid login attempt.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
            if (result.Succeeded)
            {
                var token = GenerateJwtTokenAsync(user);
                return Ok(new { token });
            }

            return BadRequest("Invalid login attempt.");
        }
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header is missing or invalid.");
            }

            var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(accessToken, _tokenValidationParameters, out var _);
                var user = await _userManager.GetUserAsync(principal);

                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);

                var claims = new Dictionary<string, string>
        {
            { "email", user.Email },
            { "role", roles.FirstOrDefault() } 
        };

                return Ok(claims);
            }
            catch (SecurityTokenException)
            {
                return Unauthorized("Invalid token.");
            }
        }
        
        private async Task<string> GenerateJwtTokenAsync(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]);

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                Expires = DateTime.UtcNow.AddDays(7),                                       //can change this ot change how long keys are valid
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //-----------------------------------------------------------login--------------------------------------------------------

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(AttendifySharedProjectC.Models.RegistrationModel registrationModel)
        {
            string newUserId = Guid.NewGuid().ToString();
            string securityStamp = Guid.NewGuid().ToString();
            string concurrencyStamp = Guid.NewGuid().ToString();

            string normalizedUserName = registrationModel.Name.ToUpper();
            string normalizedEmail = registrationModel.Email.ToUpper();

            var user = new IdentityUser
            {
                Id = newUserId,
                UserName = registrationModel.Name,
                NormalizedUserName = normalizedUserName,
                Email = registrationModel.Email,
                NormalizedEmail = normalizedEmail,
                SecurityStamp = securityStamp,
                ConcurrencyStamp = concurrencyStamp,
                PhoneNumber = "placeholder",
                EmailConfirmed = false, // will have to do stuff with this when email works
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0
            };

            string passwordHash = _passwordHasher.HashPassword(user, registrationModel.Password);
            user.PasswordHash = passwordHash;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // how the tables are connected in db:
            // ASPnetroles <-> ASPnetuserroles <-> ASPnetusers
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == registrationModel.Role);
            if (role == null)
            {
                return BadRequest($"Role '{registrationModel.Role}' not found.");
            }

            var originalRoleId = role.Id;

            var userRole = new IdentityUserRole<string>
            {
                UserId = newUserId,
                RoleId = role.Id == "1" ? role.Id : "1" 
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            if (originalRoleId != "1")
            {
                var userRoleVerification = new UserRoleVerification
                {
                    UserId = newUserId,
                    RequestedRole = registrationModel.Role,
                    VerificationStatus = "Pending",
                    DateRequested = DateTime.UtcNow
                };

                _context.UserRoleVerifications.Add(userRoleVerification);
                await _context.SaveChangesAsync();
            }

            return Ok("User created successfully with role.");
        }


        //Everything below this comment is email stuff.

        [HttpPost("generate-verification-token")]
        public async Task<ActionResult<string>> GenerateUniqueVerificationToken()
        {
            _logger.LogInformation("inside token generation method");
            string generatedToken;
            bool isUnique = false;

            do
            {
                generatedToken = GenerateRandomToken(6); 

                isUnique = !await _context.EmailVerificationTokens
                    .AnyAsync(t => t.VerificationToken == generatedToken);

            } while (!isUnique);

            var newToken = new EmailVerificationToken
            {
                VerificationToken = generatedToken,
                TokenCreationDate = DateTime.UtcNow       //timestamp is for future automatic removal of tokens, not yet implemented
            };

            _context.EmailVerificationTokens.Add(newToken);
            await _context.SaveChangesAsync();

            return Ok(generatedToken);
        }

        private string GenerateRandomToken(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost("verify-token")]
        public async Task<ActionResult<string>> VerifyToken([FromBody] string enteredToken)
        {
            var token = await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.VerificationToken == enteredToken);

            if (token != null)
            {
                _context.EmailVerificationTokens.Remove(token);
                await _context.SaveChangesAsync();
                return Ok("Token is valid!");
            }

            return Ok("NOPE");
        }

    }
}
