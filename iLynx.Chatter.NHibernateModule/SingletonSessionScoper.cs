using NHibernate;

namespace iLynx.Chatter.NHibernateModule
{
    /// <summary>
    /// StaticSessionScoper
    /// </summary>
    public class SingletonSessionScoper : ISessionScoper
    {
        private readonly ISessionFactory factory;
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonSessionScoper" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public SingletonSessionScoper(ISessionFactory factory)
        {
            this.factory = factory;
        }

        private static ISession session;
        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <returns></returns>
        public ISession GetSession()
        {
            return session ?? (session = factory.OpenSession());
        }
    }
}
