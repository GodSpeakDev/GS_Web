using GodSpeak.Web.Repositories;
using Ninject.Modules;

namespace GodSpeak.Web.Infrastructure
{
    public class NinjectRegistrations:NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationUserInviteRepository>().To<ApplicationUserInviteRepository>();
        }
    }
}