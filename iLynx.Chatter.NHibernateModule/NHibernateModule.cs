using System;
using System.Data.SQLite;
using System.IO;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.NHibernateModule.Mappings;
using iLynx.Common;
using iLynx.Common.DataAccess;
using iLynx.Configuration;
using Microsoft.Practices.Unity;
using NHibernate.Tool.hbm2ddl;

namespace iLynx.Chatter.NHibernateModule
{
    public class NHibernateModule : ModuleBase
    {
        private IConfigurableValue<string> storageFile;
        public NHibernateModule(IUnityContainer container)
            : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            storageFile = Container.Resolve<IConfigurationManager>().GetValue("NHibernate.StorageFile", "Storage.db");
            InitStorage();
            Container.RegisterType<ISessionScoper, SingletonSessionScoper>(new ContainerControlledLifetimeManager());
            Container.RegisterType(typeof(IDataAdapter<>), typeof(NHibernateAdapter<>), new ContainerControlledLifetimeManager());
        }

        private void InitStorage()
        {
            var db = Path.Combine(Environment.CurrentDirectory, storageFile.Value);
            var connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = db,
                Version = 3,
            }.ConnectionString;

            var factory =
                Fluently.Configure()
                        .Database(SQLiteConfiguration.Standard.ConnectionString(connectionString).AdoNetBatchSize(20))
                        .Mappings(
                        mc =>
                            mc.FluentMappings.AddFromAssemblyOf<UserMap>()).ExposeConfiguration(c => new SchemaUpdate(c).Execute(false, true)).BuildSessionFactory();
            Container.RegisterInstance(factory, new ContainerControlledLifetimeManager());
        }
    }
}
