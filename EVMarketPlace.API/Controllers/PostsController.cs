using EVMarketPlace.Repositories.Options;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EVMarketPlace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PostsController : ControllerBase
{
    private readonly IPostService _svc;
    private readonly IOptions<PaginationOptions> _pg;

    public PostsController(IPostService svc, IOptions<PaginationOptions> pg)
    {
        _svc = svc;
        _pg = pg;
    }

    // GET /api/Posts?keyword=&type=&page=&pageSize=

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PostListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? keyword,
        [FromQuery] string? type,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct = default)
    {
        int p = page ?? _pg.Value.DefaultPage;
        int ps = pageSize ?? _pg.Value.DefaultPageSize;

        var (total, items) = await _svc.GetListAsync(keyword, type, p, ps, ct);
        // Cho FE đọc nhanh tổng bản ghi
        Response.Headers["X-Total-Count"] = total.ToString();
        return Ok(new PagedResponse<PostListItemDto>(items, p, ps, total));
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
        

        try
        {
            var dto = await _svc.CreateAsync(req, ct);
            
            return CreatedAtAction(nameof(GetById), new { id = dto.PostId }, dto);
        }
        catch (ArgumentException ex)
        {

            return BadRequest(new ProblemDetails { Title = "Invalid request", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest req, CancellationToken ct = default)
    {

        try
        {
            var dto = await _svc.UpdateAsync(id, req, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid request", Detail = ex.Message, Status = StatusCodes.Status400BadRequest });
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
