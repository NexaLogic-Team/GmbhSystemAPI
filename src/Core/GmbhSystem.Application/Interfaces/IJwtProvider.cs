using GmbhSystem.Domain.Entities;

namespace GmbhSystem.Application.Interfaces;

public interface IJwtProvider
{
    string Generate(User user);
}