using TaskMasterPro.Application.DTOs;

namespace TaskMasterPro.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
}
