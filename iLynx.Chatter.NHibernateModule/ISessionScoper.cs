using NHibernate;

namespace iLynx.Chatter.NHibernateModule
{
    /// <summary>
    /// ISessionScoper
    /// </summary>
    public interface ISessionScoper
    {
        IStatelessSession GetSession();
    }
}
