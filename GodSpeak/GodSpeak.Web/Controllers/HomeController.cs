using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GodSpeak.Web.Repositories;
using Microsoft.AspNet.Identity;

namespace GodSpeak.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IApplicationUserProfileRepository _profileRepository;

        public HomeController(ApplicationUserManager userManager, IApplicationUserProfileRepository profileRepository)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
        }


        [Authorize]
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Your GodSpeak Dashboard";

            var userId = User.Identity.GetUserId();
            var profile = await _profileRepository.GetByUserId(userId);
            ViewBag.InviteCode = profile.Code;
            ViewBag.UserFirstName = profile.FirstName;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SignUp()
        {
            return View();
        }
    }
}