﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using Microsoft.AspNet.Identity;

namespace GodSpeak.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IApplicationUserProfileRepository _profileRepository;
        private readonly IInviteRepository _inviteRepository;
        private readonly IImpactRepository _impactRepository;
        private readonly IInMemoryDataRepository _inMemoryDataRepository;

        public HomeController(ApplicationUserManager userManager, IApplicationUserProfileRepository profileRepository,
            IInviteRepository inviteRepository, IImpactRepository impactRepository, IInMemoryDataRepository inMemoryDataRepository)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
            _inviteRepository = inviteRepository;
            _impactRepository = impactRepository;
            _inMemoryDataRepository = inMemoryDataRepository;
        }


        [Authorize]
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Your GodSpeak Dashboard";

            var userId = User.Identity.GetUserId();
            var profile = await _profileRepository.GetByUserId(userId);
            await UpdateViewBag(profile);
            
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> PurchasedAndroidInvites(PayPalTransaction transaction)
        {
            return RedirectToAction("Index");
        }

        private async Task UpdateViewBag(ApplicationUserProfile profile)
        {
            ViewBag.InviteCode = profile.Code;
            ViewBag.UserFirstName = profile.FirstName;

            var unpurchasedAndroidRequests = await _inviteRepository.UnBoughtGifts(profile.Code,
                PhonePlatforms.Android);
            ViewBag.UnpurchasedAndroidRequests = unpurchasedAndroidRequests;
            ViewBag.PurchasedAndroidRequests = await _inviteRepository.BoughtGifts(profile.Code, PhonePlatforms.Android);

            ViewBag.UnpurchasediOSRequests = await _inviteRepository.UnBoughtGifts(profile.Code, PhonePlatforms.iPhone);
            ViewBag.PurchasediOSRequests = await _inviteRepository.BoughtGifts(profile.Code, PhonePlatforms.iPhone);

            var bundles = await _inviteRepository.Bundles();
            var matchingBundle = bundles.First(b => b.NumberOfInvites >= unpurchasedAndroidRequests.Count);
            ViewBag.OutstandingAndroidBalance =
                $"{Math.Round(((float) matchingBundle.Cost / (float) matchingBundle.NumberOfInvites) * unpurchasedAndroidRequests.Count, 2):#.00}";

            var impactDays = (await _impactRepository.GetImpactForUserId(profile.UserId));
            List<string> points = new List<string>();
            if (impactDays.Any())
            {
                var latestImpactDay = impactDays.Last();

                points.AddRange(latestImpactDay.Points.Select(point => $"{{ \"point\":{{ \"lat\" :{point.Latitude}, \"lng\":{point.Longitude} }}, \"label\" : \"{point.Count}\" }}"));
            }
            var geoPoint = _inMemoryDataRepository.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];
            var usersPoint = $"{{ \"point\":{{ \"lat\" :{geoPoint.Latitude}, \"lng\":{geoPoint.Longitude} }}, \"label\" : \"1\" }}";
            points.Add(usersPoint);
            
            ViewBag.PointsJS = $"[{String.Join(",", points)}]";
            ViewBag.UserPoint = usersPoint;

            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            ViewBag.ImpactShareUrl = $"{baseUrl}/Map/{profile.Code}";
            ViewBag.ImpactEmbedCode = $"<iframe width='100%' height='100%' src='{baseUrl}/Map/{profile.Code}'></iframe>";
            Debug.WriteLine("Test");
        }

        [HttpPost]
        public async Task<ActionResult> Index(SignUpViewModel model)
        {
            ViewBag.Title = "Your GodSpeak Dashboard";

            var profile = await _profileRepository.GetByCode(model.InviteCode);
            ViewBag.UserFirstName = profile.FirstName;

            if (!ModelState.IsValid)
            {
                await UpdateViewBag(profile);
                return View(model);
            }

            if (await _inviteRepository.HasUserAlreadyRequestedGift(model.Email))
            {
                ViewBag.ResultMessage = "Sorry, that email has already registered for a gift.";
                await UpdateViewBag(profile);
                return View();
            }

            await _inviteRepository.RegisterGiftRequest(model.Email, model.Platform, model.InviteCode);

            ViewBag.ResultMessage = "Congrats! You have successfully registered a user.";
            ModelState.Clear();
            await UpdateViewBag(profile);
            return View();

        }
        [HttpGet]
        public async Task<ActionResult> DeleteRequest(Guid id)
        {
            await _inviteRepository.DeleteGiftRequest(id);
            return RedirectToAction("Index");
        }
    }
}