using System;

namespace iLynx.Chatter.Infrastructure.Domain
{
    public class EntityBase : IEntity
    {
        public virtual Guid UniqueId { get; set; }
    }
}