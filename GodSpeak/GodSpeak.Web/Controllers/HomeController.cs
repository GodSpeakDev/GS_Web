using System;
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
        private readonly IPayPalTransactionRepository _palTransactionRepository;
        private readonly IIdentityMessageService _messageService;

        public HomeController(ApplicationUserManager userManager, IApplicationUserProfileRepository profileRepository,
            IInviteRepository inviteRepository, IImpactRepository impactRepository, IInMemoryDataRepository inMemoryDataRepository, IPayPalTransactionRepository palTransactionRepository, IIdentityMessageService messageService)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
            _inviteRepository = inviteRepository;
            _impactRepository = impactRepository;
            _inMemoryDataRepository = inMemoryDataRepository;
            _palTransactionRepository = palTransactionRepository;
            _messageService = messageService;
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
            var message = "";
            if (!ModelState.IsValid)
                message = "Submission is missing required info.";
            var transactionAlreadyProcess = await _palTransactionRepository.TranscationExists(transaction.PayPalPaymentId);
            if (transactionAlreadyProcess)
                message = "Sorry, this transaction was already completed";

            if (ModelState.IsValid && !transactionAlreadyProcess)
            {
                transaction.DateTimePurchased = DateTime.Now;
                
                await _palTransactionRepository.SaveTransactions(transaction);
                var profile = await _profileRepository.GetByCode(transaction.InviteCode);
                var numberOfInvitesToCredit = (await _inviteRepository.UnBoughtGifts(transaction.InviteCode, PhonePlatforms.Android)).Count - profile.InviteBalance;
                profile.InviteBalance +=
                    numberOfInvitesToCredit;

                await _profileRepository.Update(profile);
                return RedirectToAction("ShareAndroidGift");
            }

            return RedirectToAction("Index", new {message});
        }

        private async Task UpdateViewBag(ApplicationUserProfile profile)
        {
            ViewBag.InviteCode = profile.Code;
            ViewBag.UserFirstName = profile.FirstName;

            var unpurchasedAndroidRequests = await _inviteRepository.UnBoughtGifts(profile.Code,
                PhonePlatforms.Android);
            ViewBag.UnpurchasedAndroidRequests = unpurchasedAndroidRequests;
            ViewBag.PurchasedAndroidRequests = await _inviteRepository.BoughtGifts(profile.Code, PhonePlatforms.Android);
            ViewBag.TotalPurchasedAndroidRequests = profile.InviteBalance + ViewBag.PurchasedAndroidRequests.Count;

            ViewBag.UnpurchasediOSRequests = await _inviteRepository.UnBoughtGifts(profile.Code, PhonePlatforms.iPhone);
            ViewBag.PurchasediOSRequests = await _inviteRepository.BoughtGifts(profile.Code, PhonePlatforms.iPhone);
            ViewBag.AndroidGiftBalance = profile.InviteBalance;

            var bundles = await _inviteRepository.Bundles();
            var matchingBundle = bundles.First(b => b.NumberOfInvites >= unpurchasedAndroidRequests.Count);
            ViewBag.OutstandingAndroidBalance =
                $"{Math.Round(((float) matchingBundle.Cost / (float) matchingBundle.NumberOfInvites) * (unpurchasedAndroidRequests.Count - profile.InviteBalance), 2):#.00}";
             
            var impactDays = (await _impactRepository.GetImpactForUserId(profile.UserId));
            List<string> points = new List<string>();
            if (impactDays.Any())
            {
                foreach (var impactDay in impactDays)
                {
                    points.AddRange(
                        impactDay.Points.Select(
                            point =>
                                $"{{ \"point\":{{ \"lat\" :{point.Latitude}, \"lng\":{point.Longitude} }}, \"label\" : \"{point.Count}\" }}"));
                }
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
        public async Task<ActionResult> ShareAndroidGift()
        {
            var userId = User.Identity.GetUserId();
            var profile = await _profileRepository.GetByUserId(userId);
            ViewBag.UserFirstName = profile.FirstName;
            ViewBag.InviteBalance = profile.InviteBalance;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ShareAndroidGift(ShareAndroidGiftViewModel vm)
        {
            var userId = User.Identity.GetUserId();
            var profile = await _profileRepository.GetByUserId(userId);
            var peopleToEmail = await _inviteRepository.UnBoughtGifts(profile.Code, PhonePlatforms.Android);

            foreach (var giftRequest in peopleToEmail)
            {
                await _messageService.SendAsync(new IdentityMessage()
                {
                    Body       = $"{vm.Message} \r\rYou can redeem your gift by downloading the GodSpeak app from the PlayStore (https://play.google.com/store/apps/details?id=com.givegodspeak.android) and entering GIFT CODE: {profile.Code}",
                    Destination = giftRequest.Email,
                    Subject = $"You Have Been Gifted GodSpeak by {profile.FirstName} {profile.LastName}"
                });
            }
            
            return RedirectToAction("Index", new {message = "You have successully notified your Android Gift recepients"});
        }

        [HttpGet]
        public async Task<ActionResult> DeleteRequest(Guid id)
        {
            await _inviteRepository.DeleteGiftRequest(id);
            return RedirectToAction("Index");
        }
    }
}