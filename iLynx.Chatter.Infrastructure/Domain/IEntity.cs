using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLynx.Chatter.Infrastructure.Domain
{
    public interface IEntity
    {
        Guid UniqueId { get; set; }
    }
}
