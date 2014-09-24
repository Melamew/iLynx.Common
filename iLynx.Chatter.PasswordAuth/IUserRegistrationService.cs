using iLynx.Chatter.Infrastructure.Domain;

namespace iLynx.Chatter.AuthenticationModule
{
    public interface IUserRegistrationService
    {
        bool RegisterUser(string username, string password);
        bool IsRegistered(string username, out User user);
    }
}