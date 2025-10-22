using Azure;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }
        [HttpPost("create/{postId:guid}")]
        public async Task<IActionResult> CreateFavorite([FromRoute] Guid postId)
        {
            var response = await _favoriteService.createFavorite(postId);
            return StatusCode(int.Parse(response.Status), response);

        }

        [HttpDelete("delete/{favoriteId:guid}")]
        public async Task<IActionResult> DeleteFavorite([FromRoute] Guid favoriteId)
        {
            var response = await _favoriteService.deleteFavorite(favoriteId);
            return StatusCode(int.Parse(response.Status), response);

        }
    }
}
