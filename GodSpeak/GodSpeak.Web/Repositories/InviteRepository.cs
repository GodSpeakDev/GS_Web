using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
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

        Task<bool> BundleExists(Guid guid);

        Task<InviteBundle> BundleByGuid(Guid guid);

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

        public async Task<InviteBundle> BundleByGuid(Guid guid)
        {
            return await _dbContext.InviteBundles.FindAsync(guid);
        }
        
        public async Task<bool> BundleExists(Guid guid)
        {

            return await BundleByGuid(guid) != null;
        }

       
    }
}