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
        Task RecordImpact(DateTime date, string postalCode, string countryCode, string emailAddress);

        Task RecordDeliveredMessage(DateTime date, string verseCode, string inviteCode, string userId);

        Task<List<ImpactDay>> GetImpactForUserId(string userId);

        Task<List<ImpactDay>> All();
    }
    public class ImpactRepository:IImpactRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRegistrationUtil _regUtility;
        private readonly IInMemoryDataRepository _memoryDataRepository;
        private readonly ApplicationUserManager _userManager;

        public ImpactRepository(ApplicationDbContext context, UserRegistrationUtil regUtility, IInMemoryDataRepository memoryDataRepository, ApplicationUserManager userManager)
        {
            _context = context;
            _regUtility = regUtility;
            _memoryDataRepository = memoryDataRepository;
            _userManager = userManager;
        }

        public async Task RecordImpact(DateTime date, string postalCode, string countryCode, string emailAddress)
        {
            var emailsToUpdate = new List<string>();
            if (emailAddress != "impact@godspeak.com")
            {
                emailsToUpdate = await _regUtility.GetParentEmailAddresses(emailAddress);
            
            }
            emailsToUpdate.Add(emailAddress);
            foreach (var address in emailsToUpdate)
            {
                var impactDay = await GetImpactDay(date, address);
                UpdateImpactDayPoints(postalCode, countryCode, impactDay);

                try
                {
                    _context.Entry(impactDay).State = EntityState.Modified;
                    _context.SaveChanges();
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
                await _context.ImpactDays.FirstOrDefaultAsync(d => d.DayTitle == dateKey && d.EmailAddress == code);

            if (impactDay != null) return impactDay;

            impactDay = new ImpactDay() {EmailAddress = code, Day = date.Date, DayTitle = dateKey};
            _context.ImpactDays.Add(impactDay);
            await _context.SaveChangesAsync();

            return impactDay;
        }

        protected string GetDateKey(DateTime date)
        {
            return date.ToString("MMMM dd yyyy");
        }

        public async Task<List<ImpactDay>> GetImpactForUserId(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var days = await _context.ImpactDays.Where(d => d.EmailAddress == user.Email).OrderBy(d => d.Day).ToListAsync();
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