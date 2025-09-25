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
        Task<CreateAccountRespone> CreateAccount(CreateAccountRequest request);
    }
}
