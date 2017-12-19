using System;
using System.Data.Entity;
using System.Linq;

namespace GodSpeak.Web.Extensions
{
    
    public static class DbContextExtensions
    {
        public static Boolean HasPendingChanges(this DbContext context)
        {
            return context.ChangeTracker.Entries()
                          .Any(e => e.State == EntityState.Added
                                 || e.State == EntityState.Deleted
                                 || e.State == EntityState.Modified);
        }
    }
}