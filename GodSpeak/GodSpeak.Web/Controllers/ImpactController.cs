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

        public ImpactController(IImpactRepository impactRepo, IAuthRepository authRepo, IApplicationUserProfileRepository profileRepo, UserRegistrationUtil regUtil):base(authRepo)
        {
            _impactRepo = impactRepo;
            _authRepo = authRepo;
            _profileRepo = profileRepo;
            _regUtil = regUtil;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("days")]
        public async Task<HttpResponseMessage> Days(string inviteCode)
        {
            var days = (await _impactRepo.GetImpactForInviteCode(inviteCode)).ToList().Select(ImpactApiObject.FromModel).ToList();
            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                if (i != 0)
                    day.Points = day.Points.Concat(days[i - 1].Points).ToList();

            }

            return CreateResponse(HttpStatusCode.OK, "Impact", $"Impact for code {inviteCode}",
                days);
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("message")]
        public async Task<HttpResponseMessage> RegisterMessageDelivered(DeliveredMessageRequestApiObject messageRequest)
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var userId = await _authRepo.GetUserIdForToken(GetAuthToken(Request));
            var profile = await _profileRepo.GetByUserId(userId);

            var parentCodes = await _regUtil.GetParentInviteCodes(profile.Code);
            foreach (var parentCode in parentCodes)
                await _impactRepo.RecordDeliveredMessage(messageRequest.DateDelivered, messageRequest.VerseCode, parentCode, userId);
            
            return CreateResponse(HttpStatusCode.OK, "Impact", "Delivered message has been accounted for");
        }




    }
}
