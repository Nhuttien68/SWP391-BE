using EVMarketPlace.Repositories.Entity;
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
    public class FavoriteService : IFavoriteService
    {
        private readonly FavoriteRepositori _favoriteRepositori;
        private readonly UserUtility _userUtility;
        public FavoriteService( FavoriteRepositori favoriteRepositori, UserUtility userUtility)
        {
            _favoriteRepositori = favoriteRepositori;
            _userUtility = userUtility;
        }

        public async  Task<BaseResponse> createFavorite(Guid postId)
        {
            var userid = _userUtility.GetUserIdFromToken();
            if(userid == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "UserId not found"
                };
            }
            var newFavorite = new Favorite
            {
                FavoriteId = Guid.NewGuid(),
                PostId = postId,
                UserId = userid,
                CreatedAt = DateTime.UtcNow,
            };
            await _favoriteRepositori.CreateAsync(newFavorite);
            return new BaseResponse
            {
                Status = StatusCodes.Status201Created.ToString(),
                Message = "Crate Favorite success",
            };
        }

        public async Task<BaseResponse> deleteFavorite(Guid reviewId)
        {
            var favorite = await _favoriteRepositori.GetByIdAsync(reviewId);
            if (favorite == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Favorite not found"
                };
            }
            await _favoriteRepositori.RemoveAsync(favorite);
            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Delete favorite success"
            };
        }

      
    }
}
