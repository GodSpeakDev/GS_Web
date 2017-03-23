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

        Task<ApplicationUser> FindUserByAuthToken(string token);

        string CalculateMd5Hash(string input);

        Task<bool> UserWithTokenExists(string token);

        Task<string> GetUserIdForToken(string token);

    }

    public class AuthRepository:IAuthRepository
    {
        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public AuthRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

      

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await _userManager.FindAsync(userName, password);
            return user == null ? null : (await _dbContext.Profiles.FirstAsync(p => p.ApplicationUser.Id == user.Id)).ApplicationUser;
            //HACK: for some reason _userManager isn't returning updated user
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

        

        public async Task<bool> UserWithTokenExists(string token)
        {
            return await _dbContext.Users.AnyAsync(u => u.Profile.Token == token);
        }

        public async Task<string> GetUserIdForToken(string token)
        {
            return (await _dbContext.Users.FirstAsync(u => u.Profile.Token == token)).Id;
        }

        public async Task<ApplicationUser> FindUserByAuthToken(string token)
        {
            return await _dbContext.Users.FirstAsync(u => u.Profile.Token == token);
        }
    }
}