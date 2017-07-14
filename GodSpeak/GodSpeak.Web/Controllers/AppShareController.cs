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
        private readonly IAppShareRepository _appShareRepo;
        private readonly IIdentityMessageService _messageService;
        private readonly IAuthRepository _authRepo;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationUserProfileRepository _profileRepo;

        public AppShareController(IIdentityMessageService messageService, IAppShareRepository appShareRepo, IAuthRepository authRepository, ApplicationUserManager userManager, ApplicationUserProfileRepository profileRepository) : base(authRepository)
        {
            _appShareRepo = appShareRepo;
            _messageService = messageService;
            _authRepo = authRepository;
            _userManager = userManager;
            _profileRepo = profileRepository;

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


        [HttpGet]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("api/shared")]
        public async Task<HttpResponseMessage> Retrieve(AppShareRequestObject requestObject)
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var userId = await _authRepo.GetUserIdForToken(GetAuthToken(Request));
            var user = await _userManager.FindByIdAsync(userId);

            var referredProfiles = (await _profileRepo.All()).Where(p => p.ReferringEmailAddress == user.Email);

            var results = new List<AcceptedInviteObject>();
            foreach (var profile in referredProfiles)
            {
                var referredUser = await _userManager.FindByIdAsync(profile.UserId);
                var sharedCount = (await _profileRepo.All()).Count(p => p.ReferringEmailAddress == referredUser.Email);
                results.Add(new AcceptedInviteObject()
                {
                    EmailAddress = referredUser.Email,
                    ImageUrl = profile.PhotoUrl,
                    Title = $"{profile.LastName}, {profile.FirstName}",
                    GiftsGiven = sharedCount,
                    ButtonTitle = sharedCount > 0?"Congratulate Them":"Encourage Them",
                    SubTitle = $"Has shared GodSpeak {sharedCount} times",
                    Message = sharedCount > 0? "Awesome work on spreading the word of Christ.": "\"Blessed by your GodSpeak Gift? \r\rPAY-IT-FORWARD!\r\r Bless your friends and family! Your gift will bear good fruit, it cannot fail!\r\r\"So will My word be which goes forth from My mouth; It will not return to Me empty, without accomplishing what I desire, and without succeeding in the matter for which I sent it.\" \r- GOD",
                    Subject = "GodSpeak"
                    
                });

            }

            return CreateResponse(HttpStatusCode.OK, "App Shared", "App was shared", results);
        }


    }
}
