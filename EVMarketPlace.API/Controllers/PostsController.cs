using EVMarketPlace.Repositories.Context;
using EVMarketPlace.Repositories.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using Microsoft.Identity.Client;

namespace EVMarketPlace.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PostsController(AppDbContext db) => _db = db;

        // POST /api/Posts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest req)
        {
            // (Tuỳ chọn) validate user tồn tại
            // if (!await _db.Users.AnyAsync(u => u.UserId == req.UserId))
            //     return BadRequest("UserId không tồn tại.");

            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = req.UserId,
                Type = req.Type,
                Title = req.Title,
                Description = req.Description,
                Price = req.Price,
                IsActive = req.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            return Ok(post); // hoặc CreatedAtAction("GetById", new { id = post.PostId }, post);
        }

        // GET /api/Posts?page=1&pageSize=10&keyword=&type=&priceMin=&priceMax=
        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? type = null,
            [FromQuery] decimal? priceMin = null,
            [FromQuery] decimal? priceMax = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            var query = _db.Posts.AsNoTracking().OrderByDescending(p => p.CreatedAt).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Title.Contains(keyword) || (p.Description ?? "").Contains(keyword));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(p => p.Type == type);

            if (priceMin.HasValue) query = query.Where(p => p.Price >= priceMin.Value);
            if (priceMax.HasValue) query = query.Where(p => p.Price <= priceMax.Value);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostListItemDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Type = p.Type,
                    Title = p.Title,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // GET /api/Posts/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var dto = await _db.Posts.AsNoTracking()
                .Where(p => p.PostId == id)
                .Select(p => new PostDetailDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Type = p.Type,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound("Post not found!.");
            return Ok(dto);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePostRequest req)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null) return NotFound("Post not found!");

            if (string.IsNullOrWhiteSpace(req.Title)) return BadRequest("Title cannot be empty.");
            if (string.IsNullOrWhiteSpace(req.Type)) return BadRequest("Type cannot be empty.");
            if (req.Price < 0) return BadRequest("Price cannot be negative.");

            //update
            post.Type = req.Type;
            post.Title = req.Title;
            post.Description = req.Description;
            post.Price = req.Price;
            post.IsActive = req.IsActive;

            await _db.SaveChangesAsync();

            //return detail of updated post
            var dto = new PostDetailDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Type = post.Type,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                IsActive = post.IsActive,
                CreatedAt = post.CreatedAt
            };
            return Ok(dto);
        }
            [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null) return NotFound("Post không tồn tại.");

            _db.Posts.Remove(post);            

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
