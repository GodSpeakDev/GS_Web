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

namespace GodSpeak.Web.Controllers
{
    public class InviteController : ApiControllerBase
    {
        private readonly IInviteRepository _inviteRepository;

        public InviteController(IInviteRepository inviteRepository)
        {
            _inviteRepository = inviteRepository;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        public async Task<HttpResponseMessage> Validate(string inviteCode)
        {
            if (!await _inviteRepository.InviteCodeIsValid(inviteCode))
                return CreateResponse(HttpStatusCode.NotFound, "Invalid Invite Code", "The submitted invite code is invalid");

            if (!await _inviteRepository.InviteCodeHasBalance(inviteCode))
                return CreateResponse(HttpStatusCode.PaymentRequired, "Invite Code No Balance",
                    "The submitted invite code does not have a remaining balance");

            return CreateResponse(HttpStatusCode.OK, "Invite Code Valid",
                "Congratulations, the submitted invite code is valid");

        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<List<InviteBundle>> ))]
        public async Task<HttpResponseMessage> Bundles()
        {
            return CreateResponse(HttpStatusCode.OK, "Invite Bundles", "Request for Invite Bundles succeeded", await _inviteRepository.Bundles());
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse))]
        public async Task<HttpResponseMessage> Request(EmailRequestApiObject emailRequest)
        {
            if (emailRequest == null || !ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Invite Request Failed",
                    "Please submit a valid email address");

            //TODO: Actually email the user an invite code.
            return CreateResponse(HttpStatusCode.OK, "Invite Request",
                "Congratulations, we had an extra invite. Please check your email.");
        }
    }
}
