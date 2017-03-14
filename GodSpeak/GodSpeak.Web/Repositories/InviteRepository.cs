using System.Data.Entity;
using System.Threading.Tasks;
using GodSpeak.Web.Models;
using Microsoft.AspNet.Identity;

namespace GodSpeak.Web.Repositories
{
    public interface IInviteRepository
    {
        Task<bool> InviteCodeIsValid(string inviteCode);

        Task<bool> InviteCodeHasBalance(string inviteCode);
    }

    public class InviteRepository:IInviteRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public InviteRepository(UserManager<ApplicationUser> userManager)
        {

            _userManager = userManager;
        }

        public async Task<bool> InviteCodeIsValid(string inviteCode)
        {
            return await _userManager.Users.AnyAsync(u => u.Profile.Code == inviteCode);

        }

        public async Task<bool> InviteCodeHasBalance(string inviteCode)
        {
            return (await _userManager.Users.FirstAsync(u => u.Profile.Code == inviteCode)).Profile.InviteBalance > 0;
        }

    }
}