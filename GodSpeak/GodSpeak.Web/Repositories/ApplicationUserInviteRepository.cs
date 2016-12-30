using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{
    public class ApplicationUserInviteRepository:IApplicationUserInviteRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserInviteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUserInvite> GetById(Guid id)
        {
            return await _context.Invites.FindAsync(id);
        }

        public async Task<List<ApplicationUserInvite>> All()
        {
            return await _context.Invites.ToListAsync();
        }

        public void Delete(ApplicationUserInvite model)
        {
            _context.Invites.Remove(model);
        }

        public async Task Update(ApplicationUserInvite model)
        {
            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUserInvite> GetByCode(string id)
        {
            return await _context.Invites.FirstOrDefaultAsync(invite => invite.Code == id);
        }
    }
}