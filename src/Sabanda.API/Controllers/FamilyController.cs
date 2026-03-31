using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sabanda.Application.Families.Commands;
using Sabanda.Application.Families.DTOs;
using Sabanda.Application.Families.Queries;

namespace Sabanda.API.Controllers;

[ApiController]
[Route("api/v1/families")]
[Authorize]
public class FamilyController : ControllerBase
{
    private readonly CreateFamilyCommandHandler _createHandler;
    private readonly GetFamilyQueryHandler _getHandler;
    private readonly GetFamilySummaryQueryHandler _summaryHandler;

    public FamilyController(
        CreateFamilyCommandHandler createHandler,
        GetFamilyQueryHandler getHandler,
        GetFamilySummaryQueryHandler summaryHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _summaryHandler = summaryHandler;
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateFamilyRequest request)
    {
        var response = await _createHandler.HandleAsync(request);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var response = await _getHandler.HandleAsync(id);
        return Ok(response);
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid id)
    {
        var response = await _summaryHandler.HandleAsync(id);
        return Ok(response);
    }
}
