using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AttendifySharedProjectC.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AttendifyServerProjectC.Controllers
{
    [ApiController]
    [Route("api/adminuser")]
    public class AdminUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUserController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
            var verification = await _context.UserRoleVerifications.FirstOrDefaultAsync(v => v.UserId == userId);
            if (verification == null)
            {
                return NotFound();
            }

            verification.VerificationStatus = "Accepted";

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);
            if (userRole != null)
            {
                var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == verification.RequestedRole);
                if (newRole != null)
                {
                    userRole.RoleId = newRole.Id; 
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
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
}
