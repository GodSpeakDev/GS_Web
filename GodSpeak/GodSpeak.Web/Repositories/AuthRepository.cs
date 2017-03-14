using System.Data.Entity;
using System.Threading.Tasks;
using GodSpeak.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GodSpeak.Web.Repositories
{
    public interface IAuthRepository
    {
        Task<IdentityUser> FindUser(string userName, string password);
        

    }

    public class AuthRepository:IAuthRepository
    {
        
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthRepository(UserManager<ApplicationUser> userManager)
        {
            
            _userManager = userManager;
        }

      

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await _userManager.FindAsync(userName, password);

            return user;
        }

        public void Dispose()
        {
            
            _userManager.Dispose();

        }

       
    }
}