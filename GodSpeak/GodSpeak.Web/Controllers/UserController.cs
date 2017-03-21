using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using GodSpeak.Web.Util;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;

namespace GodSpeak.Web.Controllers
{
    [RoutePrefix("api")]
    public class UserController : ApiControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInviteRepository _inviteRepository;
        private readonly IApplicationUserProfileRepository _profileRepository;
        private readonly UserRegistrationUtil _regUtil;

        public UserController(IAuthRepository authRepository, UserManager<ApplicationUser> userManager, IInviteRepository inviteRepository, IApplicationUserProfileRepository profileRepository, UserRegistrationUtil regUtil)
        {
            _authRepository = authRepository;
            _userManager = userManager;
            _inviteRepository = inviteRepository;
            _profileRepository = profileRepository;
            _regUtil = regUtil;
        }

      

        [HttpPost]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("User/Login")]
        public async Task<HttpResponseMessage> Login(LoginApiObject loginApi)
        {
            var user = await _authRepository.FindUser(loginApi.Email, loginApi.Password);
            if (user == null)
                return CreateResponse(HttpStatusCode.Forbidden, "Login Invalid", "Submitted credentials are invalid");
            return CreateResponse(HttpStatusCode.OK, "Login Valid", "Submitted credentials were valid", UserApiObject.FromModel((ApplicationUser)user));
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("User")]
        public async Task<HttpResponseMessage> Register(RegisterUserObject registerUserObject)
        {
            
            if (!ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    $"The request was missing valid data:\n {string.Join("\n",GetModelErrors())}");

            if (registerUserObject.Password != registerUserObject.PasswordConfirm)
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "The submitted passwords do not match");

            if (!await _inviteRepository.InviteCodeIsValid(registerUserObject.InviteCode))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "Submitted invite code is not valid");

            if (!await _inviteRepository.InviteCodeHasBalance(registerUserObject.InviteCode))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "Submitted invite code does not have a balance");
            
            if (await _userManager.Users.AnyAsync(u => u.Email == registerUserObject.EmailAddress))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "User with submitted email already exists");

            try
            {
                _userManager.Create(new ApplicationUser()
                {
                    Email = registerUserObject.EmailAddress,
                    UserName = registerUserObject.EmailAddress
                }, registerUserObject.Password);
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "Registration Failure",
                    "Something went wrong trying create user security record.", ex);
            }

            var user = await _userManager.Users.FirstAsync(u => u.Email == registerUserObject.EmailAddress);

            var profile = new ApplicationUserProfile
            {
                MessageCategorySettings = _regUtil.GenerateDefaultMessageCategorySettings(),
                MessageDayOfWeekSettings = _regUtil.GenerateDefaultDayOfWeekSettingsForUser(),
                FirstName = registerUserObject.FirstName,
                LastName = registerUserObject.LastName,
                CountryCode = registerUserObject.CountryCode,
                PostalCode = registerUserObject.PostalCode,
                ReferringCode = registerUserObject.InviteCode,
                Code = _regUtil.GenerateInviteCode(),
                ApplicationUser = (ApplicationUser) user,
                Token = _authRepository.CalculateMd5Hash(user.Id + user.Email),
                InviteBalance = 0
            };
            ((ApplicationUser) user).Profile = profile;
            
            profile.ApplicationUser = (ApplicationUser) user;
            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception profileException)
            {
                await _userManager.DeleteAsync(user);
                return CreateResponse(HttpStatusCode.InternalServerError, "Registration Failure",
                    "Something went wrong trying create user profile.", profileException);
            }

            return CreateResponse(HttpStatusCode.OK, "Registration Success", "User was successfully registered", UserApiObject.FromModel((ApplicationUser)user));
        }

    }

   
}
