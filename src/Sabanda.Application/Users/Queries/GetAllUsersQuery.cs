using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Users.DTOs;

namespace Sabanda.Application.Users.Queries;

public class GetAllUsersQueryHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentTenantService _tenant;

    public GetAllUsersQueryHandler(
        IUserRepository userRepository,
        ICurrentTenantService tenant)
    {
        _userRepository = userRepository;
        _tenant = tenant;
    }

    public async Task<List<UserResponse>> HandleAsync()
    {
        var users = await _userRepository.GetAllByTenantAsync(_tenant.TenantId);
        return users.Select(u => new UserResponse(u.Id, u.Email, u.Role.ToString())).ToList();
    }
}
