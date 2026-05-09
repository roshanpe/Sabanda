using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sabanda.Application.Members.Commands;
using Sabanda.Application.Members.DTOs;
using Sabanda.Application.Members.Queries;

namespace Sabanda.API.Controllers;

[ApiController]
[Authorize]
public class MemberController : ControllerBase
{
    private readonly CreateMemberCommandHandler _createHandler;
    private readonly GetMemberQueryHandler _getHandler;
    private readonly GetAllMembersQueryHandler _getAllHandler;

    public MemberController(
        CreateMemberCommandHandler createHandler,
        GetMemberQueryHandler getHandler,
        GetAllMembersQueryHandler getAllHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _getAllHandler = getAllHandler;
    }

    [HttpPost("api/v1/families/{familyId:guid}/members")]
    public async Task<IActionResult> Create(Guid familyId, [FromBody] CreateMemberRequest request)
    {
        var response = await _createHandler.HandleAsync(familyId, request);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpGet("api/v1/members")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _getAllHandler.HandleAsync();
        return Ok(response);
    }

    [HttpGet("api/v1/members/{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var response = await _getHandler.HandleAsync(id);
        return Ok(response);
    }
}
