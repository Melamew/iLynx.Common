using iLynx.Chatter.Infrastructure.Domain;

namespace iLynx.Chatter.AuthenticationModule
{
    public interface IPasswordHashingService
    {
        string GetPasswordHash(string password, out long salt);
        bool PasswordMatches(string password, User user);
    }
}