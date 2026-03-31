using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sabanda.Application.Memberships.Commands;
using Sabanda.Application.Memberships.DTOs;

namespace Sabanda.API.Controllers;

[ApiController]
[Route("api/v1/memberships")]
[Authorize(Policy = "RequireStaff")]
public class MembershipController : ControllerBase
{
    private readonly CreateMembershipCommandHandler _createHandler;
    private readonly UpdatePaymentStatusCommandHandler _updatePaymentHandler;

    public MembershipController(
        CreateMembershipCommandHandler createHandler,
        UpdatePaymentStatusCommandHandler updatePaymentHandler)
    {
        _createHandler = createHandler;
        _updatePaymentHandler = updatePaymentHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMembershipRequest request)
    {
        var response = await _createHandler.HandleAsync(request);
        return StatusCode(201, response);
    }

    [HttpPatch("{id:guid}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusRequest request)
    {
        var response = await _updatePaymentHandler.HandleAsync(id, request);
        return Ok(response);
    }
}
