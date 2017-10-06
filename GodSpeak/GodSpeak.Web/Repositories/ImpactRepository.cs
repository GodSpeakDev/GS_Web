using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GodSpeak.Web.Extensions;
using GodSpeak.Web.Models;
using GodSpeak.Web.Util;
using Microsoft.Owin.Logging;
using WebGrease.Css.Extensions;

namespace GodSpeak.Web.Repositories
{
    public interface IImpactRepository
    {
        Task RecordImpact(DateTime date, string postalCode, string countryCode, string emailAddress);

        Task RecordDeliveredMessage(DateTime date, string verseCode, string inviteCode, string userId);

        Task<List<ImpactDay>> GetImpactForUserId(string userId);

        Task<List<ImpactDay>> All();

        Task<ImpactDay> GetGodSpeakImpact();
    }
    public class ImpactRepository:IImpactRepository
    {
        private readonly ApplicationUserProfileRepository _profileRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserRegistrationUtil _regUtility;
        private readonly IInMemoryDataRepository _memoryDataRepository;
        private readonly ApplicationUserManager _userManager;

        public ImpactRepository(ApplicationDbContext context, UserRegistrationUtil regUtility, IInMemoryDataRepository memoryDataRepository, ApplicationUserManager userManager, ApplicationUserProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        
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
            var point =
                (_memoryDataRepository.PostalCodeGeoCache.ContainsKey($"{countryCode}-{postalCode}"))
                    ? _memoryDataRepository.PostalCodeGeoCache[$"{countryCode}-{postalCode}"]
                    : new PostalCodeGeoLocation()
                    {
                        Latitude = _memoryDataRepository.NoPostalCodeGeoCache[countryCode].Latitude,
                        Longitude = _memoryDataRepository.NoPostalCodeGeoCache[countryCode].Longitude,
                    };
            if (impactDay.Points == null)
                impactDay.Points = new List<ImpactDayGeoPoint>();
            impactDay.Points.Add(new ImpactDayGeoPoint() {Latitude = point.Latitude, Longitude = point.Longitude});
           
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

        public async Task<ImpactDay> GetGodSpeakImpact()
        {
            var profilesInNetwork = await _profileRepository.All();
            var impactDay = new ImpactDay();
            impactDay.Points = new List<ImpactDayGeoPoint>();
            impactDay.DeliveredMessages = new List<ImpactDeliveredMessage>();
            var scriptureCount = 0;
            foreach (var profile in profilesInNetwork)
            {
                var point =
                (_memoryDataRepository.PostalCodeGeoCache.ContainsKey($"{profile.CountryCode}-{profile.PostalCode}"))
                    ? _memoryDataRepository.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"]
                    : new PostalCodeGeoLocation()
                    {
                        Latitude = _memoryDataRepository.NoPostalCodeGeoCache[profile.CountryCode].Latitude,
                        Longitude = _memoryDataRepository.NoPostalCodeGeoCache[profile.CountryCode].Longitude,
                    };
                var numOfDaysForProfile = (DateTime.Now.Date - profile.DateRegistered.Date).TotalDays;
                scriptureCount += profile.MessageDayOfWeekSettings.First().NumOfMessages * (int)numOfDaysForProfile;

                if(!impactDay.Points.Any(p => p.Latitude == point.Latitude && p.Longitude == point.Longitude))
                {
                    impactDay.Points.Add(new ImpactDayGeoPoint()
                    {
                        Latitude = point.Latitude,
                        Longitude = point.Longitude,
                        Count = 1
                    });
                }
                else
                {
                    impactDay.Points.First(p => p.Latitude == point.Latitude && p.Longitude == point.Longitude).Count +=
                        1;
                }
                
                
            }
            for (int i = 0; i <= scriptureCount; i++)
            {
                impactDay.DeliveredMessages.Add(new ImpactDeliveredMessage());
            }
            return impactDay;
        }

        public async Task<List<ImpactDay>> GetImpactForUserId(string userId)
        {
            
            var user = await _userManager.FindByIdAsync(userId);
            var userProfile = await _profileRepository.GetByUserId(userId);

            
            var profilesInNetwork = new List<ApplicationUserProfile>();
            await addChildProfiles(profilesInNetwork, user);

            var days = new List<ImpactDay>();

            

            var startDate = userProfile.DateRegistered.Date;
            var endDate = DateTime.Now;
            var currentDate = startDate;
            var impactDay = new ImpactDay();
            while (currentDate <= endDate.Date)
            {
                var deliveredMessages = new List<ImpactDeliveredMessage>();

//                if (impactDay.DeliveredMessages != null)
//                    deliveredMessages = impactDay.DeliveredMessages.ToList();

                impactDay = new ImpactDay();
                impactDay.Day = currentDate;
                impactDay.DeliveredMessages = deliveredMessages;
                impactDay.Points = new List<ImpactDayGeoPoint>();
                days.Add(impactDay);
                //add user's delivered messages to day
                for (var i = 0; i < userProfile.MessageDayOfWeekSettings.First().NumOfMessages; i++)
                    impactDay.DeliveredMessages.Add(new ImpactDeliveredMessage());

                
                var userInNetworkForDate = profilesInNetwork.Where(p => p.DateRegistered.Date <= currentDate).ToList();
                //add users in network's delivered messages to impact day
                foreach (var networkUser in userInNetworkForDate)
                {
                    var numOfMessagesToAdd = networkUser.MessageDayOfWeekSettings.First().NumOfMessages;

                    
                    if (currentDate.Date == endDate.Date)
                    {
                        var timeInActiveSpan =
                            DateTime.Now.TimeOfDay.Subtract(networkUser.MessageDayOfWeekSettings.First().StartTime);
                        if (timeInActiveSpan.TotalSeconds > 0)
                        {
                            var totalSpan = networkUser.MessageDayOfWeekSettings.First().EndTime -
                                            networkUser.MessageDayOfWeekSettings.First().StartTime;

                            var percentOfMessagesDelivered = (float) timeInActiveSpan.TotalSeconds / (float) totalSpan.TotalSeconds;

                            numOfMessagesToAdd =
                                (int)Math.Round(percentOfMessagesDelivered *
                                           networkUser.MessageDayOfWeekSettings.First().NumOfMessages);
                        }
                    }

                    for (var i = 0; i < numOfMessagesToAdd; i++)
                        impactDay.DeliveredMessages.Add(new ImpactDeliveredMessage());

                    //check if network user's point needs to be added
                    var pointKey = $"{networkUser.CountryCode}-{networkUser.PostalCode}";
                    if (!_memoryDataRepository.PostalCodeGeoCache.ContainsKey(pointKey))
                    {
                        Debug.WriteLine($"Bad Country/Postal Code combo{pointKey}");
                    }
                    if (networkUser.DateRegistered.Date < userProfile.DateRegistered.Date && (_memoryDataRepository.PostalCodeGeoCache.ContainsKey(pointKey) || _memoryDataRepository.NoPostalCodeGeoCache.ContainsKey(networkUser.CountryCode)) )
                    {
                        var firstDay = days.First();

                        var point =  (_memoryDataRepository.PostalCodeGeoCache.ContainsKey(pointKey))
                           ? _memoryDataRepository.PostalCodeGeoCache[pointKey]
                           : new PostalCodeGeoLocation()
                           {
                               Latitude = _memoryDataRepository.NoPostalCodeGeoCache[networkUser.CountryCode].Latitude,
                               Longitude = _memoryDataRepository.NoPostalCodeGeoCache[networkUser.CountryCode].Longitude,
                           };
                        

                        if(firstDay.Points.All(p => p.ProfileId != networkUser.ApplicationUserProfileId))
                            firstDay.Points.Add(new ImpactDayGeoPoint()
                            {
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                ProfileId = networkUser.ApplicationUserProfileId
                            }
                       );
                    }
                    if (networkUser.DateRegistered.Date == currentDate.Date && (_memoryDataRepository.PostalCodeGeoCache.ContainsKey(pointKey) || _memoryDataRepository.NoPostalCodeGeoCache.ContainsKey(networkUser.CountryCode)))
                    {
                        var point = (_memoryDataRepository.PostalCodeGeoCache.ContainsKey(pointKey))
                           ? _memoryDataRepository.PostalCodeGeoCache[pointKey]
                           : new PostalCodeGeoLocation()
                           {
                               Latitude = _memoryDataRepository.NoPostalCodeGeoCache[networkUser.CountryCode].Latitude,
                               Longitude = _memoryDataRepository.NoPostalCodeGeoCache[networkUser.CountryCode].Longitude,
                           };
                        impactDay.Points.Add(new ImpactDayGeoPoint()
                            {
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                ProfileId = networkUser.ApplicationUserProfileId
                            }
                        );

                    }
                }

                currentDate = currentDate.AddDays(1);
            }

           

            return days;
        }

       
        private async Task addChildProfiles(List<ApplicationUserProfile> profilesInNetwork, ApplicationUser applicationUser)
        {
            if (applicationUser == null)
                return;
            var childProfiles = GetChildProfiles(applicationUser.Email);
            foreach (var childProfile in childProfiles)
            {
                if (profilesInNetwork.All(p => p.ApplicationUserProfileId != childProfile.ApplicationUserProfileId))
                {
                    profilesInNetwork.Add(childProfile);
                    await addChildProfiles(profilesInNetwork, (await _userManager.FindByIdAsync(childProfile.UserId)));
                }
            }
        }

        protected List<ApplicationUserProfile> GetChildProfiles(string emailAddress)
        {
            return _context.Profiles.Where(p => p.ReferringEmailAddress == emailAddress).ToList();
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