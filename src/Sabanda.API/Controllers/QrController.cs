using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sabanda.Application.Qr.Commands;
using Sabanda.Application.Qr.Queries;

namespace Sabanda.API.Controllers;

[ApiController]
[Authorize]
public class QrController : ControllerBase
{
    private readonly QrLookupQueryHandler _lookupHandler;
    private readonly RegenerateFamilyQrCommandHandler _regenFamilyHandler;
    private readonly RegenerateMemberQrCommandHandler _regenMemberHandler;

    public QrController(
        QrLookupQueryHandler lookupHandler,
        RegenerateFamilyQrCommandHandler regenFamilyHandler,
        RegenerateMemberQrCommandHandler regenMemberHandler)
    {
        _lookupHandler = lookupHandler;
        _regenFamilyHandler = regenFamilyHandler;
        _regenMemberHandler = regenMemberHandler;
    }

    [HttpGet("api/v1/qr/lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string token)
    {
        var response = await _lookupHandler.HandleAsync(token);
        return Ok(response);
    }

    [HttpPost("api/v1/families/{familyId:guid}/qr/regenerate")]
    public async Task<IActionResult> RegenerateFamily(Guid familyId)
    {
        var token = await _regenFamilyHandler.HandleAsync(familyId);
        return Ok(new { token });
    }

    [HttpPost("api/v1/members/{memberId:guid}/qr/regenerate")]
    public async Task<IActionResult> RegenerateMember(Guid memberId)
    {
        var token = await _regenMemberHandler.HandleAsync(memberId);
        return Ok(new { token });
    }
}
