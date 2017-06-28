using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{
    public class SignUpController : Controller
    {
        private readonly IApplicationUserProfileRepository _profileRepository;
        private readonly IInviteRepository _inviteRepository;

        public SignUpController(IApplicationUserProfileRepository profileRepository, IInviteRepository inviteRepository)
        {
            _profileRepository = profileRepository;
            _inviteRepository = inviteRepository;
        }
        
        [Route("SignUp/{inviteCode}")]
        public async Task<ActionResult> Index(string inviteCode)
        {
            var profile = await _profileRepository.GetByCode(inviteCode);
            ViewBag.Name = $"{profile.FirstName} {profile.LastName}";

            if (!await _inviteRepository.InviteCodeIsValid(inviteCode))
                return HttpNotFound();
            ViewBag.Title = "Receive the Gift of GodSpeak";
            ViewBag.InviteCode = inviteCode;

            

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(SignUpViewModel model, string returnUrl)
        {

            var profile = await _profileRepository.GetByCode(model.InviteCode);
            ViewBag.Name = $"{profile.FirstName} {profile.LastName}";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _inviteRepository.HasUserAlreadyRequestedGift(model.Email))
            {
                ViewBag.ResultMessage = "Sorry, that email has already registered for a gift.";
                return View();
            }

            await _inviteRepository.RegisterGiftRequest(model.Email, model.Platform, model.InviteCode);

            ViewBag.ResultMessage = "Congrats! You have successfully registered for a gift.";
            return View();

        }
    }
}