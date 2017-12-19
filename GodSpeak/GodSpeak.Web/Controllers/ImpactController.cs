using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using GodSpeak.Web.Util;

namespace GodSpeak.Web.Controllers
{
    [Route("api/impact/{action}")]
    public class ImpactController : ApiControllerBase
    {
        private readonly IImpactRepository _impactRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IApplicationUserProfileRepository _profileRepo;
        private readonly UserRegistrationUtil _regUtil;
        private readonly ApplicationUserManager _userManager;

        public ImpactController(IImpactRepository impactRepo, IAuthRepository authRepo, IApplicationUserProfileRepository profileRepo, UserRegistrationUtil regUtil, ApplicationUserManager userManager) :base(authRepo)
        {
            _impactRepo = impactRepo;
            _authRepo = authRepo;
            _profileRepo = profileRepo;
            _regUtil = regUtil;
            _userManager = userManager;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("days")]
        public async Task<HttpResponseMessage> Days(string userId)
        {
         
            var days = (await _impactRepo.GetImpactForUserId(userId)).ToList().Select(ImpactApiObject.FromModel).ToList();
            
            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                if (i != 0)
                    day.Points = day.Points.Concat(days[i - 1].Points).ToList();

            }

            return CreateResponse(HttpStatusCode.OK, "Impact", $"Impact for code {userId}",
                days);
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("didyouknow")]
        public async Task<HttpResponseMessage> DidYouKnow()
        {
            var giftsDelivered = (await _profileRepo.All()).Count;
            var deliveredMessagesCount = (await _impactRepo.All()).Sum(i => i.DeliveredMessages.Count);
            return CreateResponse(HttpStatusCode.OK, "Impact", "Did You Know",
                $"GodSpeak has been gifted {giftsDelivered} times.\rGodSpeak has delivered {deliveredMessagesCount} scriptures.");
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("message")]
        public async Task<HttpResponseMessage> RegisterMessageDelivered(DeliveredMessageRequestApiObject messageRequest)
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var userId = await _authRepo.GetUserIdForToken(GetAuthToken(Request));
            var user = await _userManager.FindByIdAsync(userId);
            

            var emailAddresses = await _regUtil.GetParentEmailAddresses(user.Email);
            foreach (var address in emailAddresses)
                await _impactRepo.RecordDeliveredMessage(messageRequest.DateDelivered, messageRequest.VerseCode, address, userId);
            
            return CreateResponse(HttpStatusCode.OK, "Impact", "Delivered message has been accounted for");
        }


       
    }
}
