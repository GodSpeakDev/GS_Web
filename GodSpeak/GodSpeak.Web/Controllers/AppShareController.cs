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
using Microsoft.AspNet.Identity;

namespace GodSpeak.Web.Controllers
{
    [Route("api/share/{action}")]
    public class AppShareController : ApiControllerBase
    {
        private IAppShareRepository _appShareRepo;
        private IIdentityMessageService _messageService;

        public AppShareController(IIdentityMessageService messageService, IAppShareRepository appShareRepo, IAuthRepository authRepository) : base(authRepository)
        {
            _appShareRepo = appShareRepo;
            _messageService = messageService;
        }


        [HttpPost]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("api/share")]
        public async Task<HttpResponseMessage> Create(AppShareRequestObject requestObject)
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            if (!ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "App Share Failure",
                    $"The request was missing valid data:\n {string.Join("\n", GetModelErrors())}");

            foreach (var toAddress in requestObject.ToEmailAddresses)
            {
                await _appShareRepo.RecordShare(requestObject.FromEmailAddress, toAddress);

                await _messageService.SendAsync(new IdentityMessage()
                {
                    
                    Destination = toAddress,
                    Subject = requestObject.Subject,
                    Body = requestObject.Message
                });
            }

            return CreateResponse(HttpStatusCode.OK, "App Shared", "App was shared");
        }

        
    }
}
