﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{

    [Route("api/invite/{action}")]
    public class InviteController : ApiControllerBase
    {
        private readonly IInviteRepository _inviteRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IApplicationUserProfileRepository _profileRepository;

        public InviteController(IInviteRepository inviteRepository, IAuthRepository authRepository, IApplicationUserProfileRepository profileRepository)
        {
            _inviteRepository = inviteRepository;
            _authRepository = authRepository;
            _profileRepository = profileRepository;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
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
        [ResponseType(typeof(ApiResponse<List<InviteBundle>>))]
        public async Task<HttpResponseMessage> Bundles()
        {
            return CreateResponse(HttpStatusCode.OK, "Invite Bundles", "Request for Invite Bundles succeeded",
                await _inviteRepository.Bundles());
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

        private HttpResponseMessage CreateMissingTokenResponse()
        {
            return CreateResponse(HttpStatusCode.Forbidden, "Request Failed", "Request is missing required header");
        }

        private async Task<bool> RequestHasValidAuthToken(HttpRequestMessage request)
        {
            const string tokenKey = "token";
            if (!request.Headers.Contains(tokenKey))
                return false;


            return await _authRepository.UserWithTokenExists(GetAuthToken(request));
        }

        private static string GetAuthToken(HttpRequestMessage request)
        {
            return request.Headers.GetValues("token").First();
        }
    }
}