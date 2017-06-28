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

        Task<bool> HasUserAlreadyRequestedGift(string emailAddress);

        Task RegisterGiftRequest(string emailAddress, PhonePlatforms platform, string referringCode);

        Task DeleteGiftRequest(Guid id);

        Task<List<GiftRequest>> UnBoughtGifts(string code, PhonePlatforms platform);

        Task<List<GiftRequest>> BoughtGifts(string code, PhonePlatforms platform);

    }

    public class InviteRepository : IInviteRepository
    {

        private readonly IApplicationUserProfileRepository _profileRepo;
        private readonly ApplicationDbContext _dbContext;
        private readonly ApplicationUserManager _userManager;

        public InviteRepository(IApplicationUserProfileRepository profileRepo, ApplicationDbContext dbContext, ApplicationUserManager userManager)
        {

            _profileRepo = profileRepo;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<bool> InviteCodeIsValid(string inviteCode)
        {
            return (await _profileRepo.All()).Any(u => u.Code == inviteCode);

        }

        public async Task<bool> InviteCodeHasBalance(string inviteCode)
        {
            return (await _profileRepo.GetByCode(inviteCode)).InviteBalance > 0;
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

        public async Task<bool> HasUserAlreadyRequestedGift(string emailAddress)
        {
            if (await _dbContext.GiftRequests.AnyAsync(req => req.Email.ToLower() == emailAddress.ToLower()))
                return true;

            if (DoesUserExist(emailAddress))
                return true;

            return false;
        }

        public async Task RegisterGiftRequest(string emailAddress, PhonePlatforms platform, string referringCode)
        {
            _dbContext.GiftRequests.Add(new GiftRequest()
            {
                Email = emailAddress,
                Platform = platform,
                ReferringCode = referringCode,
                DateTimeRequested = DateTime.Now
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteGiftRequest(Guid id)
        {
            var requestToDelete = await _dbContext.GiftRequests.FirstAsync(req => req.GiftRequestId == id);
            _dbContext.GiftRequests.Remove(requestToDelete);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<GiftRequest>> UnBoughtGifts(string code, PhonePlatforms platform)
        {
            var allGiftRequestsForCode = _dbContext.GiftRequests.Where(req => req.ReferringCode == code && req.Platform == platform).ToList();
            return allGiftRequestsForCode.Where(req => !DoesUserExist(req.Email)).ToList();

        }

        protected bool DoesUserExist(string email)
        {
            try
            {
                var user = _userManager.FindByEmail(email);
                return (user != null);
            }
            catch
            {
                return false;
            }

            
        }

        public async Task<List<GiftRequest>> BoughtGifts(string code, PhonePlatforms platform)
        {
            var allGiftRequestsForCode = _dbContext.GiftRequests.Where(req => req.ReferringCode == code && req.Platform == platform).ToList();
            return allGiftRequestsForCode.Where(req => DoesUserExist(req.Email)).ToList();
        }
    }
}
