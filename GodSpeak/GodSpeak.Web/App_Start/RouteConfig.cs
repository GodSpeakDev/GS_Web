using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GodSpeak.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "SignUp",
                url: "SignUp/{inviteCode}",
                defaults: new { controller = "SignUp", action = "Index", inviteCode = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Map",
                url: "Map/{inviteCode}",
                defaults: new { controller = "Map", action = "Index", inviteCode = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

          
        }
    }
}
