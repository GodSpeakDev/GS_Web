using System;
using System.Data.Entity;
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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;

namespace GodSpeak.Web.Controllers
{
    [Route("api/user/{action}")]
    public class UserController : ApiControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInviteRepository _inviteRepository;

        private readonly UserRegistrationUtil _regUtil;
        private readonly IInMemoryDataRepository _inMemoryDataRepo;
        private readonly IIdentityMessageService _messageService;
        private readonly IImpactRepository _impactRepository;

        public UserController(IAuthRepository authRepository, UserManager<ApplicationUser> userManager,
            IInviteRepository inviteRepository, UserRegistrationUtil regUtil, IInMemoryDataRepository inMemoryDataRepo, IIdentityMessageService messageService, IImpactRepository impactRepository) :base(authRepository)
        {
            var provider = new DpapiDataProtectionProvider("Sample");

            _authRepository = authRepository;
            _userManager = userManager;
            _userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("EmailConfirmation"));
            _inviteRepository = inviteRepository;

            _regUtil = regUtil;
            _inMemoryDataRepo = inMemoryDataRepo;
            _messageService = messageService;
            _impactRepository = impactRepository;
        }



        [HttpPost]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [ActionName("Login")]
        public async Task<HttpResponseMessage> Login(LoginApiObject loginApi)
        {
            var user = await _authRepository.FindUser(loginApi.Email, loginApi.Password);
            if (user == null)
                return CreateResponse(HttpStatusCode.Forbidden, "Login Invalid", "Submitted credentials are invalid");
            return CreateResponse(HttpStatusCode.OK, "Login Valid", "Submitted credentials were valid",
                UserApiObject.FromModel((ApplicationUser) user));
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [ActionName("RecoverPassword")]
        public async Task<HttpResponseMessage> RecoverPassword(string emailAddress)
        {
            var id = (await _userManager.Users.FirstAsync(u => u.Email == emailAddress)).Id;
            var token = await _userManager.GeneratePasswordResetTokenAsync(id);
            var newPassword = _regUtil.GenerateInviteCode();
            await _userManager.ResetPasswordAsync(id, token, newPassword);
            await _messageService.SendAsync(new IdentityMessage()
            {
                Destination = emailAddress,
                Subject = "Your New GodSpeak Password",
                Body = $"Your password has been reset to {newPassword}"
            });

            return CreateResponse(HttpStatusCode.OK, "Recover Password Success",
                "An email has been sent to your address with instructions on how to recover your password");
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("api/User")]
        public async Task<HttpResponseMessage> Register(RegisterUserObject registerUserObject)
        {

            if (!ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    $"The request was missing valid data:\n {string.Join("\n", GetModelErrors())}");

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

            if (
                !_inMemoryDataRepo.PostalCodeGeoCache.ContainsKey(
                    $"{registerUserObject.CountryCode}-{registerUserObject.PostalCode}"))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "Country and/or Postal code is invalid.");

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
                Code = await GenerateUniqueInviteCode(),
                ApplicationUser = user,
                Token = _authRepository.CalculateMd5Hash(user.Id + user.Email),
                InviteBalance = 0
            };
            user.Profile = profile;

            profile.ApplicationUser = user;
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

            try
            {
                await _impactRepository.RecordImpact(DateTime.Today, profile.PostalCode, profile.CountryCode,
                    profile.ReferringCode);
            }
            catch (Exception impactException)
            {
                await _userManager.DeleteAsync(user);
                return CreateResponse(HttpStatusCode.InternalServerError, "Registration Failure",
                    "Something went wrong recording impact.", impactException);
            }

            return CreateResponse(HttpStatusCode.OK, "Registration Success", "User was successfully registered",
                UserApiObject.FromModel(user));
        }

        private async Task<string> GenerateUniqueInviteCode()
        {
            var userCode = _regUtil.GenerateInviteCode();
            while (await _userManager.Users.AnyAsync(u => u.Profile.Code == userCode))
                userCode = _regUtil.GenerateInviteCode();
            return userCode;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("api/User")]
        public async Task<HttpResponseMessage> Profile()
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            var user = await _userManager.Users.FirstAsync(u => u.Id == userId);

            return CreateResponse(HttpStatusCode.OK, "User Profile", "User Profile Retrieved Successfully",
                UserApiObject.FromModel(user));
        }


        [HttpPut]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("api/User")]
        public async Task<HttpResponseMessage> Update(UpdateUserObject updateRequestObj)
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            if (!ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "User Update Failure",
                    $"The request was missing valid data:\n {string.Join("\n", GetModelErrors())}");

            if (!string.IsNullOrEmpty(updateRequestObj.NewPassword) && updateRequestObj.NewPassword != updateRequestObj.PasswordConfirm)
                return CreateResponse(HttpStatusCode.BadRequest, "User Update Failure",
                    "The submitted new passwords do not match");

            if(string.IsNullOrEmpty(updateRequestObj.CurrentPassword) && !string.IsNullOrEmpty(updateRequestObj.NewPassword))
                return CreateResponse(HttpStatusCode.BadRequest, "User Update Failure",
                                   "Request is missing current password for changing password");

            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            var user = await _userManager.Users.FirstAsync(u => u.Id == userId);
            
            //update password if needed
            if(!string.IsNullOrEmpty(updateRequestObj.CurrentPassword) && await _authRepository.FindUser(user.UserName, updateRequestObj.CurrentPassword) == null)
                return CreateResponse(HttpStatusCode.BadRequest, "User Update Failure",
                                   "Submitted current password was incorrect");

            if (!string.IsNullOrEmpty(updateRequestObj.NewPassword))
               await _userManager.ChangePasswordAsync(userId, updateRequestObj.CurrentPassword, updateRequestObj.NewPassword);

            
            UpdateProfileProps(updateRequestObj, user);

            
            try
            {
                UpdateMessageCategorySettings(updateRequestObj, user);
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "User Update Failure",
                    "Something went wrong updating message category settings", ex);
            }
            
            try
            {
                UpdateMessageDayOfWeekSettings(updateRequestObj, user);
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "User Update Failure",
                    "Something went wrong updating day of week message settings", ex);
            }

            //save changes to DB
            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                CreateResponse(HttpStatusCode.InternalServerError, "User Update Failure",
                    "Something went wrong trying to update the user in the database", ex);
            }

            return CreateResponse(HttpStatusCode.OK, "User Update Success", "User was successfully updated",
                UserApiObject.FromModel(user));
        }

        private static void UpdateMessageDayOfWeekSettings(UpdateUserObject updateRequestObj, ApplicationUser user)
        {
            foreach (var setting in updateRequestObj.MessageDayOfWeekSettings)
            {
                var existingSetting =
                    user.Profile.MessageDayOfWeekSettings.First(s => s.MessageDayOfWeekSettingId == setting.Id);
                existingSetting.Enabled = setting.Enabled;
                existingSetting.StartTime = setting.StartTime;
                existingSetting.EndTime = setting.EndTime;
                existingSetting.NumOfMessages = setting.NumOfMessages;
            }
        }

        private static void UpdateMessageCategorySettings(UpdateUserObject updateRequestObj, ApplicationUser user)
        {
            foreach (var setting in updateRequestObj.MessageCategorySettings)
                user.Profile.MessageCategorySettings.First(s => s.MessageCategorySettingId == setting.Id).Enabled =
                    setting.Enabled;
        }

        private static void UpdateProfileProps(UpdateUserObject updateUserObject, ApplicationUser user)
        {
            user.Profile.FirstName = updateUserObject.FirstName;
            user.Profile.LastName = updateUserObject.LastName;
            user.Profile.CountryCode = updateUserObject.CountryCode;
            user.Profile.PostalCode = updateUserObject.PostalCode;
        }
    }

}
