using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GodSpeak.Web.Extensions;
using GodSpeak.Web.Models;
using GodSpeak.Web.Util;

namespace GodSpeak.Web.Repositories
{
    public interface IImpactRepository
    {
        Task RecordImpact(DateTime date, string postalCode, string countryCode, string inviteCode);

        Task<List<ImpactDay>> GetImpactForInviteCode(string inviteCode);
    }
    public class ImpactRepository:IImpactRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRegistrationUtil _regUtility;
        private readonly IInMemoryDataRepository _memoryDataRepository;

        public ImpactRepository(ApplicationDbContext context, UserRegistrationUtil regUtility, IInMemoryDataRepository memoryDataRepository)
        {
            _context = context;
            _regUtility = regUtility;
            _memoryDataRepository = memoryDataRepository;
        }

        public async Task RecordImpact(DateTime date, string postalCode, string countryCode, string inviteCode)
        {
            var codesToUpdate = await _regUtility.GetParentInviteCodes(inviteCode);
            foreach (var code in codesToUpdate)
            {
                UpdateImpactDayPoints(postalCode, countryCode, await GetImpactDay(date, code));

                try
                {
//                    _context.Entry(await GetImpactDay(date, code)).State = EntityState.Modified;
//                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    
                }

            }


                

        }

        private void UpdateImpactDayPoints(string postalCode, string countryCode, ImpactDay impactDay)
        {
            var geoPoint = _memoryDataRepository.PostalCodeGeoCache[$"{countryCode}-{postalCode}"];
            if (impactDay.Points == null)
                impactDay.Points = new List<ImpactDayGeoPoint>();
            impactDay.Points.Add(new ImpactDayGeoPoint() {Latitude = geoPoint.Latitude, Longitude = geoPoint.Longitude});
           
        }

        private async Task<ImpactDay> GetImpactDay(DateTime date, string code)
        {
            var dateKey = date.Date.ToUniversalTime();
            var impactDay =
                await _context.ImpactDays.FirstOrDefaultAsync(d => d.Day == dateKey && d.InviteCode == code);

            if (impactDay != null) return impactDay;

            impactDay = new ImpactDay() {InviteCode = code, Day = dateKey};
            _context.ImpactDays.Add(impactDay);

            return impactDay;
        }

        public async Task<List<ImpactDay>> GetImpactForInviteCode(string inviteCode)
        {
            return await _context.ImpactDays.Where(d => d.InviteCode == inviteCode).OrderBy(d => d.Day).ToListAsync();
        }
    }
}