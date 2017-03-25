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
    [Route("api/impact/{action}")]
    public class ImpactController : ApiControllerBase
    {
        private readonly IImpactRepository _impactRepo;

        public ImpactController(IImpactRepository impactRepo, IAuthRepository authRepo):base(authRepo)
        {
            _impactRepo = impactRepo;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("days")]
        public async Task<HttpResponseMessage> Days(string inviteCode)
        {
            return CreateResponse(HttpStatusCode.OK, "Impact", $"Impact for code {inviteCode}",
                (await _impactRepo.GetImpactForInviteCode(inviteCode)).ToList().Select(ImpactApiObject.FromModel).ToList());
        }




    }
}
