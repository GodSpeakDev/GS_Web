using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GodSpeak.Web.Startup))]
namespace GodSpeak.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
