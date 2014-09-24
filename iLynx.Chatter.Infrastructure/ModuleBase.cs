using System.Reflection;
using iLynx.Common;
using iLynx.Common.WPF;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.Infrastructure
{
    public abstract class ModuleBase : IModule
    {
        private readonly IUnityContainer container;

        protected IUnityContainer Container
        {
            get { return container; }
        }

        protected ModuleBase(IUnityContainer container)
        {
            this.container = Guard.IsNull(() => container);
        }

        public void Initialize()
        {
            RegisterTypes();
        }

        protected virtual void RegisterResource(string path)
        {
            var mergeDictionaryService = Container.Resolve<IMergeDictionaryService>();
            mergeDictionaryService.AddResource(RuntimeHelper.MakePackUri(Assembly.GetCallingAssembly(), path));
        }

        protected abstract void RegisterTypes();
    }
}
