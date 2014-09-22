using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Authentication;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.AuthenticationModule
{
    public abstract class AuthModuleBase : ModuleBase
    {
        protected AuthModuleBase(IUnityContainer container) : base(container)
        {
        }

        protected override void RegisterTypes()
        {
            throw new NotImplementedException();
        }

        protected abstract void RegisterAuthHandler(ICompositeAuthenticationHandler<ChatMessage, int> compositeHandler);
    }
}
