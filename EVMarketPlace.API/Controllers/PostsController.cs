using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{

    ///  Controller CRUD cho Post
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _PostService;
        public PostsController(IPostService postService)
        {
            _PostService = postService; 
        }

        // Lấy danh sách Post
        //cancellationToken ct = default là để hủy tác vụ khi client ngắt request (ví dụ: đóng tab, timeout) giúp tiết kiệm tài nguyên.
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)

        {
            var item = await _PostService.GetAllAsync(ct);
            return Ok(item);
        }


        //Lấy chi tiết Post theo id
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
        {
            var item = await _PostService.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        // Tạo mới Post.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostCreateRequest req, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _PostService.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.PostId }, created);
        }

        // Cập nhật Post 

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] PostUpdateRequest req, CancellationToken ct = default)
        {
            req.PostId = id; // đồng bộ id từ route
            try
            {
                var updated = await _PostService.UpdateAsync(req, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Xóa Post theo id
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
        {
            var ok = await _PostService.DeleteAsync(id, ct);
            return ok ? Ok(new { deleted = true }) : NotFound();
        }
    }
}
