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
    public class InviteController : ApiController
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

        private HttpResponseMessage CreateResponse(HttpStatusCode responseStatusCode, string responseTitle, string responseMessage)
        {
            return Request.CreateResponse(responseStatusCode, new ApiResponse()
            {
                Title = responseTitle,
                Message = responseMessage
            });
        }
    }
}
