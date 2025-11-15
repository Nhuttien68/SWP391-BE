using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IAuctionService
    {
        Task<BaseResponse> CreateAuctionAsync( CreateAuctionRequest req);
        Task<BaseResponse> PlaceBidAsync( PlaceBidRequest req);
        Task<List<AuctionCloseResultDTO>> CloseExpiredAuctionsAsync();
        Task<BaseResponse> UpdateTransactionReceiverInfoAsync(Guid transactionId, UpdateTransactionRequest req);
        Task<BaseResponse?> GetAuctionByIdAsync(Guid auctionId);
        Task<List<Auction>> GetActiveAuctionsAsync();
    }
}
