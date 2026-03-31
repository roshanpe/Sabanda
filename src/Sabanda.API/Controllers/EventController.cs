using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sabanda.Application.Events.Commands;
using Sabanda.Application.Events.DTOs;

namespace Sabanda.API.Controllers;

[ApiController]
[Route("api/v1/events")]
[Authorize(Policy = "RequireStaff")]
public class EventController : ControllerBase
{
    private readonly CreateEventCommandHandler _createHandler;
    private readonly RegisterEventCommandHandler _registerHandler;
    private readonly CancelEventRegistrationCommandHandler _cancelHandler;

    public EventController(
        CreateEventCommandHandler createHandler,
        RegisterEventCommandHandler registerHandler,
        CancelEventRegistrationCommandHandler cancelHandler)
    {
        _createHandler = createHandler;
        _registerHandler = registerHandler;
        _cancelHandler = cancelHandler;
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var response = await _createHandler.HandleAsync(request);
        return StatusCode(201, response);
    }

    [HttpPost("{id:guid}/registrations")]
    public async Task<IActionResult> Register(Guid id, [FromBody] RegisterEventRequest request)
    {
        var response = await _registerHandler.HandleAsync(id, request);
        return StatusCode(201, response);
    }

    [HttpDelete("{id:guid}/registrations/{registrationId:guid}")]
    public async Task<IActionResult> CancelRegistration(Guid id, Guid registrationId)
    {
        await _cancelHandler.HandleAsync(id, registrationId);
        return NoContent();
    }
}
