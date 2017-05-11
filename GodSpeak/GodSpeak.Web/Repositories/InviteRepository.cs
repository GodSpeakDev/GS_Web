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

        
        Task<List<InviteBundle>> Bundles();

        Task<bool> BundleExists(Guid guid);

        Task<InviteBundle> BundleByGuid(Guid guid);

    }

    public class InviteRepository:IInviteRepository
    {
        
        private readonly IApplicationUserProfileRepository _profileRepo;
        private readonly ApplicationDbContext _dbContext;

        public InviteRepository(IApplicationUserProfileRepository profileRepo, ApplicationDbContext dbContext)
        {
            
            _profileRepo = profileRepo;
            _dbContext = dbContext;
        }

        public async Task<bool> InviteCodeIsValid(string inviteCode)
        {
            return (await _profileRepo.All()).Any(u => u.Code == inviteCode);

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