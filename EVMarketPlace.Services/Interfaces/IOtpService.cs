using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IOtpService
    {
        string GenerateAndSaveOtp(string email, int expireMinutes = 5);
        bool VerifyOtp(string email, string otp);
    }

}
