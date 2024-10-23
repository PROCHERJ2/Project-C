using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AttendifySharedProjectC.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AttendifyServerProjectC.Controllers
{
    [ApiController]
    [Route("api/adminuser")]
    public class AdminUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminUserController> _logger;

        public AdminUserController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AdminUserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("getusers")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(user => new UserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Id = user.Id,
                    Role = (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ur.UserId == user.Id
                            select r.Name).FirstOrDefault()
                })
                .OrderBy(u => u.UserName)
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("user-role-verifications")]
        public async Task<IActionResult> GetUserRoleVerifications()
        {
            var roleVerifications = await (from verification in _context.UserRoleVerifications
                                           join user in _context.Users on verification.UserId equals user.Id
                                           join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoleJoin
                                           from userRole in userRoleJoin.DefaultIfEmpty()
                                           join role in _context.Roles on userRole.RoleId equals role.Id into roleJoin
                                           from role in roleJoin.DefaultIfEmpty()
                                           select new RoleVerificationDto
                                           {
                                               UserId = user.Id,
                                               UserName = user.UserName,
                                               CurrentRole = role != null ? role.Name : "No role", 
                                               RequestedRole = verification.RequestedRole,
                                               VerificationStatus = verification.VerificationStatus,
                                               DateRequested = verification.DateRequested
                                           }).ToListAsync();

            return Ok(roleVerifications);
        }

        [HttpPost("accept-request")]
        public async Task<IActionResult> AcceptRequest([FromBody] string userId)
        {
            _logger.LogInformation("Inside Accept Request");
            var verification = await _context.UserRoleVerifications.FirstOrDefaultAsync(v => v.UserId == userId);

            if (verification == null)
            {
                return NotFound("Verification request not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var requestedRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == verification.RequestedRole);
            if (requestedRole == null)
            {
                return NotFound($"Requested role '{verification.RequestedRole}' not found.");
            }

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }

            var newUserRole = new IdentityUserRole<string>
            {
                UserId = userId,
                RoleId = requestedRole.Id
            };
            _context.UserRoles.Add(newUserRole);
            await _context.SaveChangesAsync();
            verification.VerificationStatus = "accepted";
            await _context.SaveChangesAsync();

            return Ok("User role updated and request accepted.");
        }


        [HttpPost("deny-request")]
        public async Task<IActionResult> DenyRequest([FromBody] string userId)
        {
            var verification = await _context.UserRoleVerifications.FirstOrDefaultAsync(v => v.UserId == userId);
            if (verification == null)
            {
                return NotFound();
            }

            verification.VerificationStatus = "Denied";

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("remove-user/{userId}")]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);

            var userRoles = _context.UserRoles.Where(ur => ur.UserId == userId);
            _context.UserRoles.RemoveRange(userRoles);
            var userVerifications = _context.UserRoleVerifications.Where(uv => uv.UserId == userId);
            _context.UserRoleVerifications.RemoveRange(userVerifications);
            await _context.SaveChangesAsync();

            return Ok();
        }



        //for changing roles

        [HttpGet("getroles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();

            return Ok(roles);
        }

        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] RoleChangeRequest model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null) return NotFound();

            // Remove current roles
            var userRoles = await _context.UserRoles.Where(ur => ur.UserId == model.UserId).ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);

            // Add new role
            _context.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = model.UserId,
                RoleId = model.NewRoleId
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        //for changing roles

    }

    public class RoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class RoleVerificationDto   // is a bit double, can and will move this to the models folder
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CurrentRole { get; set; }
        public string RequestedRole { get; set; }
        public string VerificationStatus { get; set; }
        public DateTime DateRequested { get; set; }
    }

    public class RoleChangeRequest
    {
        public string UserId { get; set; }
        public string NewRoleId { get; set; }
    }
}
