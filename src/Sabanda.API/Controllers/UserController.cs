using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sabanda.Application.Users.Queries;

namespace Sabanda.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly GetAllUsersQueryHandler _getAllHandler;

    public UserController(GetAllUsersQueryHandler getAllHandler)
    {
        _getAllHandler = getAllHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _getAllHandler.HandleAsync();
        return Ok(response);
    }
}
