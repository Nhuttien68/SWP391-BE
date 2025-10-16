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
        Task<BaseResponse> CreateAccount(CreateAccountRequest request);
        Task<BaseResponse> LoginAsync(LoginRequest request);
        Task<BaseResponse> VerifyOtpActiveAccountAsync(string email, string opt);
        
        Task<BaseResponse> ChangePasswordAsync(ChangePasswordRequest request);
        Task<BaseResponse> ForgotPasswordAsync(string email);
        Task<BaseResponse> ResendOtpAsync(string email);
    }
}
