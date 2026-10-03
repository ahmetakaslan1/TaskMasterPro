using TaskMasterPro.Domain.Entities;

namespace TaskMasterPro.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}
