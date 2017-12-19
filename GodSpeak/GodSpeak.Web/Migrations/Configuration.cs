using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using GodSpeak.Web.Util;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebGrease.Css.Extensions;

namespace GodSpeak.Web.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GodSpeak.Web.Models.ApplicationDbContext>
    {
        

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
        
       
    }
}
