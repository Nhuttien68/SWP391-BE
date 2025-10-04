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
        private readonly IPostService _svc;
        public PostsController(IPostService svc) => _svc = svc;

        // Lấy danh sách Post
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? type = null,
            CancellationToken ct = default)
        {
            var data = await _svc.GetPagedAsync(page, pageSize, keyword, type, ct);
            return Ok(data);
        }

        //Lấy chi tiết Post theo id
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
        {
            var item = await _svc.GetByIdAsync(id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        // Tạo mới Post.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostCreateRequest req, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.PostId }, created);
        }

        // Cập nhật Post 

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] PostUpdateRequest req, CancellationToken ct = default)
        {
            req.PostId = id; // đồng bộ id từ route
            try
            {
                var updated = await _svc.UpdateAsync(req, ct);
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
            var ok = await _svc.DeleteAsync(id, ct);
            return ok ? Ok(new { deleted = true }) : NotFound();
        }
    }
}
