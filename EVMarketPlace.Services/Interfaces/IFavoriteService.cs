using EVMarketPlace.Repositories.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<BaseResponse> createFavorite(Guid postId);
        Task<BaseResponse> deleteFavorite(Guid reviewId);
        //Task<BaseResponse> GetById(Guid reviewId);
        //Task<BaseResponse> GetAllFavorite();


    }
}
