using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{
    public class ApplicationUserProfileRepository:IApplicationUserProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUserProfile> GetById(Guid? id)
        {
            return await _context.Profiles.FindAsync(id);
        }

        

        public async Task<List<ApplicationUserProfile>> All()
        {
            return await _context.Profiles.ToListAsync();
        }

        public async Task Delete(ApplicationUserProfile model)
        {
            _context.Profiles.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ApplicationUserProfile model)
        {
            _context.Entry(model).State = EntityState.Modified;
         
            
            await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUserProfile> GetByCode(string id)
        {
            return await _context.Profiles.FirstOrDefaultAsync(invite => invite.Code == id);
        }

        public async Task Insert(ApplicationUserProfile model)
        {
            _context.Profiles.Add(model);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        

        public async Task<bool> ApplyInviteCredit(string userId, int numOfInvites)
        {
                var profile = await _context.Profiles.FirstAsync(p => p.UserId == userId);
                profile.InviteBalance += numOfInvites;
                await Update(profile);
                return true;
          
        }

        public async Task<ApplicationUserProfile> GetByUserId(string id)
        {
            return await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == id);
        }
    }
}