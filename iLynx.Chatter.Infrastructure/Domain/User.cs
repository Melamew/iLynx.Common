using System;

namespace iLynx.Chatter.Infrastructure.Domain
{
    public class EntityBase : IEntity
    {
        public virtual Guid UniqueId { get; set; }
    }
    
    public class User : EntityBase
    {
        public virtual string Username { get; set; }
        public virtual string PasswordHash { get; set; }
        public virtual long PasswordSalt { get; set; }
    }
}
