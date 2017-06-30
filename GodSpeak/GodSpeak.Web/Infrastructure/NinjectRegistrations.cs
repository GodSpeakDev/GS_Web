using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using GodSpeak.Web.Util;
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
            Bind<ApplicationUserManager>().ToSelf();

            Bind<UserRegistrationUtil>().ToSelf();
            Bind<IIdentityMessageService>().To<EmailService>();
            Bind<IApplicationUserProfileRepository>().To<ApplicationUserProfileRepository>();
            Bind<IInviteRepository>().To<InviteRepository>();
            Bind<IAuthRepository>().To<AuthRepository>();
            Bind<IInMemoryDataRepository>().To<InMemoryDataRepository>();
            Bind<IImpactRepository>().To<ImpactRepository>();
            Bind<IAppShareRepository>().To<AppShareRepository>();
            Bind<IPayPalTransactionRepository>().To<PayPalTransactionRepository>();
        }
    }
}