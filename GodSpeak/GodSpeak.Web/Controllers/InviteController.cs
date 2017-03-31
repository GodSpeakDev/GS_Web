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
using Microsoft.AspNet.Identity;

namespace GodSpeak.Web.Controllers
{

    [Route("api/invite/{action}")]
    public class InviteController : ApiControllerBase
    {
        private readonly IIdentityMessageService _messageService;
        private readonly ApplicationUserManager _userManager;
        private readonly IInviteRepository _inviteRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IApplicationUserProfileRepository _profileRepository;
        private readonly UserRegistrationUtil _regUtil;

        public InviteController(IIdentityMessageService messageService, ApplicationUserManager userManager, IInviteRepository inviteRepository, IAuthRepository authRepository, IApplicationUserProfileRepository profileRepository, UserRegistrationUtil regUtil):base(authRepository)
        {
            _messageService = messageService;
            _userManager = userManager;
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
                return CreateResponse(HttpStatusCode.NotFound, "Invalid Gift Code",
                    "The submitted gift code is invalid");

            if (!await _inviteRepository.InviteCodeHasBalance(inviteCode))
                return CreateResponse(HttpStatusCode.PaymentRequired, "Gift Code No Balance",
                    "The submitted gift code does not have a remaining balance");

            return CreateResponse(HttpStatusCode.OK, "Gift Code Valid",
                "Congratulations, the submitted gift code is valid");

        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("parents")]
        public async Task<HttpResponseMessage> ParentInviteCodes(string inviteCode)
        {
            return CreateResponse(HttpStatusCode.OK, "Parent Gift Codes", "Here are the codes",
                await _regUtil.GetParentInviteCodes(inviteCode));
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<List<InviteBundle>>))]
        public async Task<HttpResponseMessage> Bundles()
        {
            return CreateResponse(HttpStatusCode.OK, "Gift Bundles", "Request for Gift Bundles succeeded",
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
            var profile = (await _profileRepository.All()).First(p => p.UserId == user.Id);
            var profiles = (await _profileRepository.All()).Where(p => p.ReferringCode == profile.Code);

            var acceptedInvites = new List<AcceptedInviteObject>();
            foreach (var p in profiles)
                acceptedInvites.Add(await CreateAcceptedInvite(p));


            return CreateResponse(HttpStatusCode.OK, "Accepted Gifts", "Request for accepted gifts succeeded", acceptedInvites);
        }

        private async Task<AcceptedInviteObject> CreateAcceptedInvite(ApplicationUserProfile profile)
        {
            var acceptedInviteCount = (await _profileRepository.All()).Count(p => p.ReferringCode == profile.Code);
            var hasGifted = acceptedInviteCount > 0;
            var user = await _userManager.FindByIdAsync(profile.UserId);

            return new AcceptedInviteObject
            {
                Title = $"{profile.FirstName} {profile.LastName}",
                EmailAddress = user.Email,
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
        [ActionName("Donate")]
        [ResponseType(typeof(ApiResponse))]
        public async Task<HttpResponseMessage> Donate()
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var donorUserId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            var donorProfile = await _profileRepository.GetByUserId(donorUserId);

            if (donorProfile.InviteBalance <= 0)
                return CreateResponse(HttpStatusCode.OK, "Gift Donation Request",
                    "Sorry, you do not have any remaining gifts to donate");

            var donationsUser = _userManager.Users.First(u => u.Email == "donations@godspeakapp.com");
            var donationsProfile = await _profileRepository.GetByUserId(donationsUser.Id);

            donationsProfile.InviteBalance += 1;
            await _profileRepository.Update(donationsProfile);

            donorProfile.InviteBalance -= 1;
            await _profileRepository.Update(donorProfile);

            return CreateResponse(HttpStatusCode.OK, "Gift Request",
                "You have successfully donated a gift to the world.");
        }

        [HttpPost]
        [ActionName("Request")]
        [ResponseType(typeof(ApiResponse))]
        public async Task<HttpResponseMessage> RequestInvite(EmailRequestApiObject emailRequest)
        {
            if (emailRequest == null || !ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Gift Request Failed",
                    "Please submit a valid email address");
            var donationsAccount = _userManager.Users.First(u => u.Email == "donations@godspeakapp.com");

            var donationsProfile = await _profileRepository.GetByUserId(donationsAccount.Id);



            if (donationsProfile.InviteBalance > 0)
            {
                try
                {
                    donationsProfile.InviteBalance = donationsProfile.InviteBalance - 1;
                    await _profileRepository.Update(donationsProfile);
                }
                catch (Exception ex)
                {
                    return CreateResponse(HttpStatusCode.InternalServerError, "Gift Request",
                        "Sorry something went wrong updating our donations account", ex);
                }
            }
            else
            {
                return CreateResponse(HttpStatusCode.OK, "Gift Request",
                       "Sorry we don't have any donations available");
            }

            try
            {
                await _messageService.SendAsync(new IdentityMessage()
                {
                    Destination = emailRequest.EmailAddress,
                    Subject = "Your GodSpeak Gift Code",
                    Body = $"You can use this Gift Code to access GodSpeak {donationsProfile.Code}"
                });
            }
            catch (Exception ex)
            {
                donationsProfile.InviteBalance = donationsProfile.InviteBalance + 1;
                await _profileRepository.Update(donationsProfile);
                return CreateResponse(HttpStatusCode.InternalServerError, "Gift Request Error",
                    "Sorry, something went wrong trying to email your gift code.", ex);
            }


            return CreateResponse(HttpStatusCode.OK, "Gift Request",
                "Congratulations, we had an extra gift. Please check your email.");
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse))]
        public async Task<HttpResponseMessage> Purchase(GuidRequestApiObject guidRequest)
        {
            

            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            if (!ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Gift Purchase Failed",
                    "Request is missing gift bundle guid");

            if (!await _inviteRepository.BundleExists(guidRequest.Guid))
                return CreateResponse(HttpStatusCode.NotFound, "Gift Purchase Failed",
                    "No gift bundle was found with submitted guid");

            var bundle = await _inviteRepository.BundleByGuid(guidRequest.Guid);
            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));

            if(!await _profileRepository.ApplyInviteCredit(userId, bundle.NumberOfInvites))
                return CreateResponse(HttpStatusCode.NotFound, "Gift Purchase Failed",
                    "Something went wrong applying credit");

            return CreateResponse(HttpStatusCode.OK, "Gift Purchase Succeeded",
                "Congratulations, you successfully purchased more gifts.");
        }

       
    }
}
