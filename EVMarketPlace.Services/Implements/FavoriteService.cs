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
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public async Task<BaseResponse> deleteFavorite(Guid favoriteId)
        {
            var favorite = await _favoriteRepositori.GetByIdAsync(favoriteId);
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

        public async Task<BaseResponse> GetAllFavorite()
        {
            var userId = _userUtility.GetUserIdFromToken();
            if (userId == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status401Unauthorized.ToString(),
                    Message = "User not authenticated"
                };
            }

            var favorites = await _favoriteRepositori.GetAllFavoriteWithPostInforAsync(userId);
               

            if (favorites == null || !favorites.Any())
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "No favorites found",
                    Data = new List<object>(),
                };
            }

            var data = favorites.Select(f => new
            {
                f.FavoriteId,
                f.CreatedAt,
                Post = new
                {
                    f.Post.PostId,
                    f.Post.Title,
                    f.Post.Price,
                    f.Post.Status,
                    Images = f.Post.PostImages.Select(i => i.ImageUrl).ToList()
                }
            }).ToList();

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Get favorites success",
                Data = data
            };
        }

        public async Task<BaseResponse> GetById(Guid favoriteId)
        {
            var userId = _userUtility.GetUserIdFromToken();
            if (userId == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status401Unauthorized.ToString(),
                    Message = "User not found"
                };
            }

            var favorite = await _favoriteRepositori.GetFavoriteByIdAsync(userId, favoriteId);

            if (favorite == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Favorite not found"
                };
            }

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Get favorite successfully",
                Data = favorite
            };
        }
    }
}
