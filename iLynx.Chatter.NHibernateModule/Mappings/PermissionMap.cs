using FluentNHibernate.Mapping;
using iLynx.Chatter.Infrastructure.Domain;

namespace iLynx.Chatter.NHibernateModule.Mappings
{
    public class PermissionMap : ClassMap<Permission>
    {
        public PermissionMap()
        {
            Id(x => x.UniqueId).GeneratedBy.GuidNative();
            Map(x => x.PermissionIdentifier)
                .Index("idx_permissionIdentifier")
                .Unique();
        }
    }
}