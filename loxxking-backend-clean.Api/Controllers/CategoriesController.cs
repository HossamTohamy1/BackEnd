using loxxking_backend_clean.Application.Features.Categories.Commands.CreateCategory;
using loxxking_backend_clean.Application.Features.Categories.Commands.DeleteCategory;
using loxxking_backend_clean.Application.Features.Categories.Commands.UpdateCategory;
using loxxking_backend_clean.Application.Features.Categories.Queries.GetCategories;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;
    public CategoriesController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => (await _sender.Send(new GetCategoriesQuery(), ct)).ToApiResponse();

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug, CancellationToken ct) => (await _sender.Send(new loxxking_backend_clean.Application.Features.Categories.Queries.GetCategoryBySlug.GetCategoryBySlugQuery(slug), ct)).ToApiResponse();

    [HttpPost]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand cmd, CancellationToken ct) => (await _sender.Send(cmd with { Id = id }, ct)).ToApiResponse();

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => (await _sender.Send(new DeleteCategoryCommand(id), ct)).ToApiResponse();
}
