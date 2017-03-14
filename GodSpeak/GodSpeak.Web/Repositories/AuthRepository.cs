using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using GodSpeak.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GodSpeak.Web.Repositories
{
    public interface IAuthRepository
    {
        Task<IdentityUser> FindUser(string userName, string password);

        string CalculateMd5Hash(string input);

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

        public string CalculateMd5Hash(string input)

        {
            

            var md5 = MD5.Create();

            var inputBytes = Encoding.UTF8.GetBytes(input);

            var hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(i.ToString("x2"));
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            
            _userManager.Dispose();

        }

       
    }
}