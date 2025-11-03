using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IReviewService
    {
        Task<BaseResponse> CreateReviewForSellerAsync( ReviewCreateDTO dto);
        Task<BaseResponse> CreateReviewForPostAsync( ReviewCreateDTO dto);
        Task<BaseResponse> GetByPostIdAsync(Guid postId);
        Task<BaseResponse> GetByUserIdAsync(Guid userId);
        Task<BaseResponse> GetByTransactionIdAsync(Guid transactionId);
        Task<BaseResponse> DeleteReviewAsync(Guid reviewId);
    }
}
