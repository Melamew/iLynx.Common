using iLynx.Chatter.Infrastructure.Domain;

namespace iLynx.Chatter.Infrastructure.Services
{
    public interface IUserPermissionService
    {
        bool HasPermission(User user, Permission permission);
    }
}
