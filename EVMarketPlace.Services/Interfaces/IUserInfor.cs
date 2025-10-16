using EVMarketPlace.Repositories.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IUserInfor
    {
        Task<BaseResponse> GetByUserId(Guid userId);
        Task<BaseResponse> UpdateUserInfor(Guid userId, string fullName, string phoneNumber, string address);
    }
}
