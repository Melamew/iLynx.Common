using System.Collections.Generic;

namespace iLynx.Chatter.Infrastructure.Domain
{
    public class User : EntityBase
    {
        public virtual string Username { get; set; }
        public virtual byte[] PasswordHash { get; set; }
        public virtual long PasswordSalt { get; set; }
        public virtual IList<Permission> Permissions { get; set; }
    }
}
