using loxxking_backend_clean.Application.Features.Users.Commands.AdminChangePassword;
using loxxking_backend_clean.Application.Features.Users.Commands.ChangePassword;
using loxxking_backend_clean.Application.Features.Users.Commands.CreateSalesEmployee;
using loxxking_backend_clean.Application.Features.Users.Commands.CreateStoreManager;
using loxxking_backend_clean.Application.Features.Users.Commands.DeleteAnyUser;
using loxxking_backend_clean.Application.Features.Users.Commands.DeleteUser;
using loxxking_backend_clean.Application.Features.Users.Commands.ResetStaffPassword;
using loxxking_backend_clean.Application.Features.Users.Commands.ToggleStaffStatus;
using loxxking_backend_clean.Application.Features.Users.Commands.UpdateStaff;
using loxxking_backend_clean.Application.Features.Users.Queries.GetProfile;
using loxxking_backend_clean.Application.Features.Users.Queries.GetStaff;
using loxxking_backend_clean.Application.Features.Users.Queries.LoginUser;
using loxxking_backend_clean.Domain.Enums;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    public UsersController(ISender sender) { _sender = sender; }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserQuery query, CancellationToken ct) => (await _sender.Send(query, ct)).ToApiResponse();

    [HttpGet("me")]
[Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken ct) {
        var idString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("nameid")?.Value;
        var id = Guid.Parse(idString ?? Guid.Empty.ToString());
        return (await _sender.Send(new GetProfileQuery(id), ct)).ToApiResponse();
    }

    [HttpPost("admin/create-manager")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStoreManager([FromBody] CreateStoreManagerCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpPost("staff/create-employee")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> CreateSalesEmployee([FromBody] CreateSalesEmployeeCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpGet("staff")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> GetStaff([FromQuery] UserRole? role, CancellationToken ct) => (await _sender.Send(new GetStaffQuery(role), ct)).ToApiResponse();

    [HttpPut("staff/{id}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateStaffCommand cmd, CancellationToken ct) => (await _sender.Send(cmd with { Id = id }, ct)).ToApiResponse();

    [HttpPatch("staff/{id}/toggle-status")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> ToggleStaffStatus(Guid id, CancellationToken ct) => (await _sender.Send(new ToggleStaffStatusCommand(id), ct)).ToApiResponse();

    [HttpPost("change-password")]
[Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpPost("staff/reset-password")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> ResetStaffPassword([FromBody] ResetStaffPasswordCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpPost("admin/change-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminChangePassword([FromBody] AdminChangePasswordCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct) => (await _sender.Send(new DeleteUserCommand(id), ct)).ToApiResponse();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> DeleteAnyUser(Guid id, CancellationToken ct) => (await _sender.Send(new DeleteAnyUserCommand(id), ct)).ToApiResponse();
}
