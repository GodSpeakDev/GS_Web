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

        Task RecordDeliveredMessage(DateTime date, string verseCode, string inviteCode, string userId);

        Task<List<ImpactDay>> GetImpactForInviteCode(string inviteCode);

        Task<List<ImpactDay>> All();
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
            var dateKey = GetDateKey(date);
            var impactDay =
                await _context.ImpactDays.FirstOrDefaultAsync(d => d.DayTitle == dateKey && d.InviteCode == code);

            if (impactDay != null) return impactDay;

            impactDay = new ImpactDay() {InviteCode = code, Day = date.Date, DayTitle = dateKey};
            _context.ImpactDays.Add(impactDay);
            await _context.SaveChangesAsync();

            return impactDay;
        }

        protected string GetDateKey(DateTime date)
        {
            return date.ToString("MMMM dd yyyy");
        }

        public async Task<List<ImpactDay>> GetImpactForInviteCode(string inviteCode)
        {
            var days = await _context.ImpactDays.Where(d => d.InviteCode == inviteCode).OrderBy(d => d.Day).ToListAsync();
            return days;
        }

        public async Task RecordDeliveredMessage(DateTime date, string verseCode, string inviteCode, string userId)
        {
            var impactDay = await GetImpactDay(date, inviteCode);
            if (impactDay.DeliveredMessages == null)
                impactDay.DeliveredMessages = new List<ImpactDeliveredMessage>();
            if (impactDay.DeliveredMessages.Any(m => m.UserId == userId && m.VerseCode == verseCode))
                return;
            impactDay.DeliveredMessages.Add(new ImpactDeliveredMessage() {UserId = userId, VerseCode =  verseCode});
            _context.Entry(impactDay).State = EntityState.Modified;
            _context.SaveChanges();
            
        }

        public async Task<List<ImpactDay>> All()
        {
            return await _context.ImpactDays.ToListAsync();
        }
    }
}