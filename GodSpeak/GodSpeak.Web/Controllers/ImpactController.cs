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




    }
}
