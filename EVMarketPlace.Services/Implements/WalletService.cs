using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Exception;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class WalletService : IWalletService
    {
        private readonly UserUtility _userUtility;
        private readonly WalletRepository _walletRepository;
        public WalletService(UserUtility userUtility, WalletRepository walletRepository)
        {
            _userUtility = userUtility;
            _walletRepository = walletRepository;
        }
        public async Task<BaseResponse> CreateWallet()
        {
            var userId = _userUtility.GetUserIdFromToken();
            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User is not authenticated.");
            }
            var newWallet = new Wallet
            {
                WalletId = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                LastUpdated = DateTime.UtcNow
            };
            await _walletRepository.CreateAsync(newWallet);
            return new BaseResponse
            {
                Status = StatusCodes.Status201Created.ToString(),
                Message = "Wallet created successfully.",
                Data = new WalletResponeseDto
                {
                    WalletId = newWallet.WalletId,
                    UserId = newWallet.UserId,
                    Balance = newWallet.Balance,
                    LastUpdated = newWallet.LastUpdated
                }
            };
        }
    }
}
