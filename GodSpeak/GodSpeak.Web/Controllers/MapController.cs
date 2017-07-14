﻿using System;
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
                var scriptureCount = impactDays.Sum(d => d.DeliveredMessages.Count);
                var giftCount = 0;
                foreach (var impactDay in impactDays)
                {
                    giftCount += impactDay.Points.Count;
                    points.AddRange(impactDay.Points.Select(point => $"{{ \"point\":{{ \"lat\" :{point.Latitude}, \"lng\":{point.Longitude} }}, \"label\" : \"{point.Count}\" }}"));
                }
                

                ViewBag.ImpactText =
                    $"{profile.FirstName}'s Impact: {giftCount} Apps Gift/Shared {scriptureCount} Scriptures Delivered";
            }
            var geoPoint = _inMemoryDataRepository.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];
            var usersPoint = $"{{ \"point\":{{ \"lat\" :{geoPoint.Latitude}, \"lng\":{geoPoint.Longitude} }}, \"label\" : \"0\" }}";
            points.Add(usersPoint);

            ViewBag.PointsJS = $"[{string.Join(",", points)}]";
            ViewBag.UserPoint = usersPoint;
            ViewBag.FirstName = profile.FirstName;
            ViewBag.InviteCode = profile.Code;

            

            return View();
        }

        public async Task<ActionResult> GodSpeak()
        {
            

            ViewBag.Title = $"GodSpeak's Impact Map";

            var impactDay = await _impactRepository.GetGodSpeakImpact();
            var points = new List<string>();
            points.AddRange(impactDay.Points.Select(point => $"{{ \"point\":{{ \"lat\" :{point.Latitude}, \"lng\":{point.Longitude} }}, \"label\" : \"{point.Count}\" }}"));
            ViewBag.ImpactText =
                    $"GodSpeak's Impact: {impactDay.Points.Sum(p => p.Count + 1)} Apps Gift/Shared {impactDay.DeliveredMessages.Count} Scriptures Delivered";
            

            ViewBag.PointsJS = $"[{string.Join(",", points)}]";
            
            


            return View();
        }
    }
}