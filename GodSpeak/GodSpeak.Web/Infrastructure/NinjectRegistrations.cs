﻿using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ninject;
using Ninject.Modules;

namespace GodSpeak.Web.Infrastructure
{
    public class NinjectRegistrations:NinjectModule
    {
        public override void Load()
        {
            Bind<ApplicationDbContext>().ToSelf();
            Bind<IUserStore<ApplicationUser>>().To<UserStore<ApplicationUser>>().WithConstructorArgument("context", Kernel.Get<ApplicationDbContext>()); ;
            Bind<UserManager<ApplicationUser>>().ToSelf();

            Bind<IApplicationUserProfileRepository>().To<ApplicationUserProfileRepository>();
            Bind<IAuthRepository>().To<AuthRepository>();
        }
    }
}