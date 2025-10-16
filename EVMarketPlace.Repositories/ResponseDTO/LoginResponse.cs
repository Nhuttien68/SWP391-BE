using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.ResponseDTO
{
    public class LoginResponse
    {
        public Guid AccountId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string Token { get; set; } = null!;

        public string Status { get; set; }
    }
}
