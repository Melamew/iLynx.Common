﻿using iLynx.Chatter.Infrastructure;
using Microsoft.Practices.Unity;

namespace iLynx.Chatter.AuthenticationModule
{
    public class ClientAuthenticationModule : ModuleBase
    {
        public ClientAuthenticationModule(IUnityContainer container) : base(container)
        {

        }

        protected override void RegisterTypes()
        {
            RegisterResource("Resources/DataTemplates.xaml");

        }
    }
}