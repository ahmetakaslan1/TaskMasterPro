namespace TaskMasterPro.Application.DTOs;

public class RegisterRequest {
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class LoginRequest {
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class UserDto {
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class AuthResponseDto {
    public string Token { get; init; } = string.Empty;
    public UserDto User { get; init; } = new();
}
