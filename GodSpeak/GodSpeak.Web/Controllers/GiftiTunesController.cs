using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using Microsoft.AspNet.Identity;

namespace GodSpeak.Web.Controllers
{
    public class GiftiTunesController : Controller
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IApplicationUserProfileRepository _profileRepository;
        private readonly IInviteRepository _inviteRepository;

        public GiftiTunesController(ApplicationUserManager userManager, IApplicationUserProfileRepository profileRepository,
            IInviteRepository inviteRepository)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
            _inviteRepository = inviteRepository;
        }

        // GET: GiftiTunes
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "How To Gift via iTunes";
            try
            {
                var userId = User.Identity.GetUserId();
                var profile = await _profileRepository.GetByUserId(userId);
                var requests = await _inviteRepository.UnBoughtGifts(profile.Code, PhonePlatforms.iPhone);
                ViewBag.EmailAddresses = String.Join(",", requests.Select(req => req.Email).ToArray());
                return View();
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> Desktop()
        {
            ViewBag.Title = "How To Gift via iTunes";
             return View();
            
        }

        public async Task<ActionResult> FromAndroid()
        {
            ViewBag.Title = "How To Gift via iTunes";
            return View();

        }


    }

    
}