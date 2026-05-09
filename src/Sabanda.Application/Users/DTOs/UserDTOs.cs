namespace Sabanda.Application.Users.DTOs;

public record UserResponse(
    Guid Id,
    string Email,
    string Role
);
