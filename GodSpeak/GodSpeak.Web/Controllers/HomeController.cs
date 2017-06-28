using System;
using System.Collections.Generic;
using System.Linq;
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

        public HomeController(ApplicationUserManager userManager, IApplicationUserProfileRepository profileRepository,
            IInviteRepository inviteRepository)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
            _inviteRepository = inviteRepository;
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

        private async Task UpdateViewBag(ApplicationUserProfile profile)
        {
            ViewBag.InviteCode = profile.Code;
            ViewBag.UserFirstName = profile.FirstName;

            ViewBag.UnpurchasedAndroidRequests = await _inviteRepository.UnBoughtGifts(profile.Code,
                PhonePlatforms.Android);
            ViewBag.PurchasedAndroidRequests = await _inviteRepository.BoughtGifts(profile.Code, PhonePlatforms.Android);

            ViewBag.UnpurchasediOSRequests = await _inviteRepository.UnBoughtGifts(profile.Code, PhonePlatforms.iPhone);
            ViewBag.PurchasediOSRequests = await _inviteRepository.BoughtGifts(profile.Code, PhonePlatforms.iPhone);
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
    }
}