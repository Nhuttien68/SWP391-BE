using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.RequestDTO
{
    public class CreateAccountRequest
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string Password { get; set; } = null!;
        
    }
    public class ChangePasswordRequest
    {
        public string Email { get; set; } 
        public string Otp { get; set; }
        public string NewPassWord { get; set; }
    }

}
