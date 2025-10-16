using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController( IWalletService walletService)
        {
            _walletService = walletService;
        }
        [HttpPost("create-wallet")]
        public async Task<BaseResponse> CreateWallet()
        {
            return await _walletService.CreateWallet();
        }
    }
}
