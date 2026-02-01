using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class StubUserService : ICurrentUserService
{
    private static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000001");

    public Guid? UserId => SystemUserId;
    public bool IsAuthenticated => true;
}