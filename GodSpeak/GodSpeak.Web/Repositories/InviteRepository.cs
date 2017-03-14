using System.Collections.Generic;
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

        Task<List<InviteBundle>> Bundles();
    }

    public class InviteRepository:IInviteRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public InviteRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<bool> InviteCodeIsValid(string inviteCode)
        {
            return await _userManager.Users.AnyAsync(u => u.Profile.Code == inviteCode);

        }

        public async Task<bool> InviteCodeHasBalance(string inviteCode)
        {
            return (await _userManager.Users.FirstAsync(u => u.Profile.Code == inviteCode)).Profile.InviteBalance > 0;
        }

        public async Task<List<InviteBundle>> Bundles()
        {
            return await _dbContext.InviteBundles.ToListAsync();
        }
    }
}