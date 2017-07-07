using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{
    public class MapController : Controller
    {
        private readonly ApplicationUserProfileRepository _profileRepository;
        private readonly ImpactRepository _impactRepository;
        private readonly InMemoryDataRepository _inMemoryDataRepository;

        public MapController(ApplicationUserProfileRepository profileRepository, ImpactRepository impactRepository, InMemoryDataRepository inMemoryDataRepository)
        {
            _profileRepository = profileRepository;
            _impactRepository = impactRepository;
            _inMemoryDataRepository = inMemoryDataRepository;
        }

        // GET: Map
        public async Task<ActionResult> Index(string inviteCode)
        {
            var profile = await _profileRepository.GetByCode(inviteCode);
            
            ViewBag.Title = $"{profile.FirstName}'s Impact Map";

            var impactDays = await _impactRepository.GetImpactForUserId(profile.UserId);
            var points = new List<string>();
            if (impactDays.Any())
            {
                var latestImpactDay = impactDays.Last();

                points.AddRange(latestImpactDay.Points.Select(point => $"{{ \"point\":{{ \"lat\" :{point.Latitude}, \"lng\":{point.Longitude} }}, \"label\" : \"{point.Count}\" }}"));

                ViewBag.ImpactText =
                    $"{profile.FirstName}'s Impact: {latestImpactDay.Points.Sum(p => (p.Count == 0)?1:p.Count)} Apps Gift/Shared {latestImpactDay.DeliveredMessages.Count} Scriptures Delivered";
            }
            var geoPoint = _inMemoryDataRepository.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];
            var usersPoint = $"{{ \"point\":{{ \"lat\" :{geoPoint.Latitude}, \"lng\":{geoPoint.Longitude} }}, \"label\" : \"1\" }}";
            points.Add(usersPoint);

            ViewBag.PointsJS = $"[{string.Join(",", points)}]";
            ViewBag.UserPoint = usersPoint;
            ViewBag.FirstName = profile.FirstName;
            ViewBag.InviteCode = profile.Code;

            

            return View();
        }
    }
}