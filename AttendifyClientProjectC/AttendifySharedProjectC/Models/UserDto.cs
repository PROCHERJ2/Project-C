using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendifySharedProjectC.Models
{
    public class UserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? Role { get; set; }
        public string Id { get; set; }
    }
}
