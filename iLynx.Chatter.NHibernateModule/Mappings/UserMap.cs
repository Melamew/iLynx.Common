using FluentNHibernate.Mapping;
using iLynx.Chatter.Infrastructure.Domain;

namespace iLynx.Chatter.NHibernateModule.Mappings
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.UniqueId);
            Map(x => x.Username)
                .Not.Nullable();
            Map(x => x.PasswordHash)
                .Not.Nullable();
            Map(x => x.PasswordSalt)
                .Not.Nullable();
        }
    }
}
