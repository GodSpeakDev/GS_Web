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

    [Route("api/invite/{action}")]
    public class InviteController : ApiControllerBase
    {
        private readonly IInviteRepository _inviteRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IApplicationUserProfileRepository _profileRepository;
        private readonly UserRegistrationUtil _regUtil;

        public InviteController(IInviteRepository inviteRepository, IAuthRepository authRepository, IApplicationUserProfileRepository profileRepository, UserRegistrationUtil regUtil):base(authRepository)
        {
            _inviteRepository = inviteRepository;
            _authRepository = authRepository;
            _profileRepository = profileRepository;
            _regUtil = regUtil;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("validate")]
        public async Task<HttpResponseMessage> Validate(string inviteCode)
        {
            if (!await _inviteRepository.InviteCodeIsValid(inviteCode))
                return CreateResponse(HttpStatusCode.NotFound, "Invalid Invite Code",
                    "The submitted invite code is invalid");

            if (!await _inviteRepository.InviteCodeHasBalance(inviteCode))
                return CreateResponse(HttpStatusCode.PaymentRequired, "Invite Code No Balance",
                    "The submitted invite code does not have a remaining balance");

            return CreateResponse(HttpStatusCode.OK, "Invite Code Valid",
                "Congratulations, the submitted invite code is valid");

        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("parents")]
        public async Task<HttpResponseMessage> ParentInviteCodes(string inviteCode)
        {
            return CreateResponse(HttpStatusCode.OK, "Parent Invite Codes", "Here are the codes",
                await _regUtil.GetParentInviteCodes(inviteCode));
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<List<InviteBundle>>))]
        public async Task<HttpResponseMessage> Bundles()
        {
            return CreateResponse(HttpStatusCode.OK, "Invite Bundles", "Request for Invite Bundles succeeded",
                await _inviteRepository.Bundles());
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<AcceptedInviteObject>))]
        [ActionName("accepted")]
        public async Task<HttpResponseMessage> Accepted()
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var user = await _authRepository.FindUserByAuthToken(GetAuthToken(Request));
            var profiles = (await _profileRepository.All()).Where(p => p.ReferringCode == user.Profile.Code);

            var acceptedInvites = new List<AcceptedInviteObject>();
            foreach (var profile in profiles)
                acceptedInvites.Add(await CreateAcceptedInvite(profile));


            return CreateResponse(HttpStatusCode.OK, "Accepted Invites", "Request for accepted invites succeeded", acceptedInvites);
        }

        private async Task<AcceptedInviteObject> CreateAcceptedInvite(ApplicationUserProfile profile)
        {
            var acceptedInviteCount = (await _profileRepository.All()).Count(p => p.ReferringCode == profile.Code);
            var hasGifted = acceptedInviteCount > 0;

            return new AcceptedInviteObject
            {
                Title = $"{profile.FirstName} {profile.LastName}",
                EmailAddress = profile.ApplicationUser.Email,
                SubTitle = hasGifted
                    ? $"{acceptedInviteCount} gifts given"
                    : "Has not given any gifts",
                ButtonTitle = hasGifted
                    ? $"Congratulate {profile.FirstName}"
                    : $"Encourage {profile.FirstName}",
                Message = hasGifted
                    ? "Congrats on spreading the good word of Christ!"
                    : "Don't forget to pay it forward!"
            };
        }

        [HttpPost]
        [ActionName("Request")]
        [ResponseType(typeof(ApiResponse))]
        public async Task<HttpResponseMessage> RequestInvite(EmailRequestApiObject emailRequest)
        {
            if (emailRequest == null || !ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Invite Request Failed",
                    "Please submit a valid email address");

            //TODO: Actually email the user an invite code.
            return CreateResponse(HttpStatusCode.OK, "Invite Request",
                "Congratulations, we had an extra invite. Please check your email.");
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse))]
        public async Task<HttpResponseMessage> Purchase(GuidRequestApiObject guidRequest)
        {
            

            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            if (!ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Invite Purchase Failed",
                    "Request is missing invite bundle guid");

            if (!await _inviteRepository.BundleExists(guidRequest.Guid))
                return CreateResponse(HttpStatusCode.NotFound, "Invite Purchase Failed",
                    "No invite bundle was found with submitted guid");

            var bundle = await _inviteRepository.BundleByGuid(guidRequest.Guid);
            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));

            if(!await _profileRepository.ApplyInviteCredit(userId, bundle.NumberOfInvites))
                return CreateResponse(HttpStatusCode.NotFound, "Invite Purchase Failed",
                    "Something went wrong applying credit");

            return CreateResponse(HttpStatusCode.OK, "Invite Purchase Succeeded",
                "Congratulations, you successfully purchased more invites.");
        }

       
    }
}
