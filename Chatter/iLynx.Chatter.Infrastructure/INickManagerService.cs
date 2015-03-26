using System;

namespace iLynx.Chatter.Infrastructure
{
    public interface INickManagerService
    {
        string GetNickName(Guid clientId);
    }
}