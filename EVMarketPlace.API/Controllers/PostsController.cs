using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PostsController : ControllerBase
{
    private readonly IPostService _svc;

    public PostsController(IPostService svc)
    {
        _svc = svc;
    }

    // GET /api/Posts?keyword=&type=&page=1&pageSize=10
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PostListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? keyword,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (total, items) = await _svc.GetListAsync(keyword, type, page, pageSize, ct);
        var resp = new PagedResponse<PostListItemDto>(items, page, pageSize, total);
        return Ok(resp);
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var dto = await _svc.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }


    [HttpPost]
    [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var dto = await _svc.CreateAsync(req, ct);
            
            return CreatedAtAction(nameof(GetById), new { id = dto.PostId }, dto);
        }
        catch (ArgumentException ex)
        {
            
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var dto = await _svc.UpdateAsync(id, req, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var ok = await _svc.DeleteAsync(id, ct);
        return ok ? Ok(new { deleted = true }) : NotFound();
    }
}


public sealed record PagedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int Total
);
