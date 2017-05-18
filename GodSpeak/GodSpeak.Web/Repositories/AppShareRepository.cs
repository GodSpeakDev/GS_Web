using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{

    public interface IAppShareRepository
    {
        Task RecordShare(string fromAddress, string toAddress);

        Task<List<string>> GetReferrals(string emailAddress);
    }

    public class AppShareRepository:IAppShareRepository
    {
        private readonly ApplicationDbContext _context;

        public AppShareRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RecordShare(string fromAddress, string toAddress)
        {
            _context.AppShares.Add(new AppShare()
            {
                From = fromAddress,
                To = toAddress
            });

            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetReferrals(string emailAddress)
        {
           return await _context.AppShares.Where(s => s.To == emailAddress).Select(s => s.From).ToListAsync();
        }
    }
}