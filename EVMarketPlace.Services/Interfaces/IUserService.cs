using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseRespone> CreateAccount(CreateAccountRequest request);
        Task<BaseRespone> LoginAsync(LoginRequest request);
        Task<BaseRespone> VerifyOtpActiveAccountAsync(string email, string opt);
        
        Task<BaseRespone> ChangePasswordAsync(ChangePasswordRequest request);
        Task<BaseRespone> ForgotPasswordAsync(string email);
    }
}
