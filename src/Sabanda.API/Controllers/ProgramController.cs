using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sabanda.Application.Programs.Commands;
using Sabanda.Application.Programs.DTOs;
using Sabanda.Application.Programs.Queries;

namespace Sabanda.API.Controllers;

[ApiController]
[Route("api/v1/programs")]
[Authorize(Policy = "RequireStaff")]
public class ProgramController : ControllerBase
{
    private readonly CreateProgramCommandHandler _createHandler;
    private readonly EnrolMemberCommandHandler _enrolHandler;
    private readonly CancelEnrolmentCommandHandler _cancelHandler;
    private readonly GetAllProgramsQueryHandler _getAllHandler;

    public ProgramController(
        CreateProgramCommandHandler createHandler,
        EnrolMemberCommandHandler enrolHandler,
        CancelEnrolmentCommandHandler cancelHandler,
        GetAllProgramsQueryHandler getAllHandler)
    {
        _createHandler = createHandler;
        _enrolHandler = enrolHandler;
        _cancelHandler = cancelHandler;
        _getAllHandler = getAllHandler;
    }

    [HttpPost]
    [Authorize(Policy = "RequireStaff")]
    public async Task<IActionResult> Create([FromBody] CreateProgramRequest request)
    {
        var response = await _createHandler.HandleAsync(request);
        return StatusCode(201, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _getAllHandler.HandleAsync();
        return Ok(response);
    }

    [HttpPost("{id:guid}/enrolments")]
    public async Task<IActionResult> Enrol(Guid id, [FromBody] EnrolMemberRequest request)
    {
        var response = await _enrolHandler.HandleAsync(id, request);
        return StatusCode(201, response);
    }

    [HttpDelete("{id:guid}/enrolments/{enrolmentId:guid}")]
    public async Task<IActionResult> CancelEnrolment(Guid id, Guid enrolmentId)
    {
        await _cancelHandler.HandleAsync(id, enrolmentId);
        return NoContent();
    }
}
