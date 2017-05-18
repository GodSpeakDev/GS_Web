using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
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
        private readonly IApplicationUserProfileRepository _profileRepo;
        private readonly IAuthRepository _authRepository;
        private readonly ApplicationUserManager _userManager;
        

        private readonly UserRegistrationUtil _regUtil;
        private readonly IInMemoryDataRepository _inMemoryDataRepo;
        private readonly IIdentityMessageService _messageService;
        private readonly IImpactRepository _impactRepository;

        public UserController(IApplicationUserProfileRepository profileRepo, IAuthRepository authRepository, ApplicationUserManager userManager,
            UserRegistrationUtil regUtil, IInMemoryDataRepository inMemoryDataRepo, IIdentityMessageService messageService, IImpactRepository impactRepository) :base(authRepository)
        {
            var provider = new DpapiDataProtectionProvider("Sample");

            _profileRepo = profileRepo;
            _authRepository = authRepository;
            _userManager = userManager;
            _userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("EmailConfirmation"));
            

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
            var profile = await _profileRepo.GetByUserId(user.Id);
            profile.Token = _authRepository.CreateToken();
            await _profileRepo.Update(profile);
            var geoPoint = _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];
            return CreateResponse(HttpStatusCode.OK, "Login Valid", "Submitted credentials were valid",
                UserApiObject.FromModel((ApplicationUser) user, profile, geoPoint));
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("logout")]
        public async Task<HttpResponseMessage> Logout()
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();
            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            var profile = await _profileRepo.GetByUserId(userId);
            profile.Token = string.Empty;

            try
            {
                await _profileRepo.Update(profile);
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "User Logout Error",
                    "Something went wrong trying to update profile", ex);
            }

            return CreateResponse(HttpStatusCode.OK, "User Logged Out", "Authorization token has been cleared");
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
                UserId = user.Id,
                Token = _authRepository.CreateToken()
            };
            

            
            try
            {
                await _profileRepo.Insert(profile);
            }
            catch (Exception profileException)
            {
                await _userManager.DeleteAsync(user);
                return CreateResponse(HttpStatusCode.InternalServerError, "Registration Failure",
                    "Something went wrong trying create user profile.", profileException);
            }
            
            var geoPoint = _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];
            return CreateResponse(HttpStatusCode.OK, "Registration Success", "User was successfully registered",
                UserApiObject.FromModel(user, profile, geoPoint));
        }
        [HttpPost]
        [ResponseType(typeof(ApiResponse))]
        [Route("api/User/Referral")]
        public async Task<HttpResponseMessage> RegisterReferral(RegisterReferralApiObject referralObj)
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            if (!ModelState.IsValid)
                return CreateResponse(HttpStatusCode.BadRequest, "Referral Registration Failure",
                    $"The request was missing valid data:\n {string.Join("\n", GetModelErrors())}");

            if(await _userManager.FindByEmailAsync(referralObj.ReferringEmailAddress) == null)
                return CreateResponse(HttpStatusCode.BadRequest, "Referral Registration Failure",
                    $"{referralObj.ReferringEmailAddress} does not match any existing users");


            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            var profile = await _profileRepo.GetByUserId(userId);

            profile.ReferringEmailAddress = referralObj.ReferringEmailAddress;

            await _profileRepo.Update(profile);

            return CreateResponse(HttpStatusCode.OK, "User", "Referral has been registered");
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

            var profile = await _profileRepo.GetByUserId(user.Id);
            var geoPoint = _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];
            return CreateResponse(HttpStatusCode.OK, "User Profile", "User Profile Retrieved Successfully",
                UserApiObject.FromModel(user, profile, geoPoint));
        }

        [HttpPost]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("api/user/photo")]
        public async Task<HttpResponseMessage> UploadPhoto()
        {
            try
            {
                if (!await RequestHasValidAuthToken(Request))
                    return CreateMissingTokenResponse();

                var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
                var user = await _userManager.Users.FirstAsync(u => u.Id == userId);
                var profile = await _profileRepo.GetByUserId(userId);

                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                foreach (var file in provider.Contents)
                {

                    try
                    {
                        var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                        var imageName = $"{Guid.NewGuid()}-{Path.GetFileName(filename)}";
                        var profileImagesFolderPath =
                            System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Profile_Images");
                        if (!Directory.Exists(profileImagesFolderPath))
                            Directory.CreateDirectory(profileImagesFolderPath);

                        var path = Path.Combine(profileImagesFolderPath,
                            imageName);
                        var buffer = await file.ReadAsByteArrayAsync();
                        File.WriteAllBytes(path, buffer);
                        profile.PhotoUrl = "/Content/Profile_Images/" + imageName;

                        await _profileRepo.Update(profile);
                    }
                    catch (Exception ex)
                    {
                        return CreateResponse(HttpStatusCode.InternalServerError, "User Profile",
                            "There was an error trying to save the file.", ex);
                    }

                }



                var geoPoint = _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];


                return CreateResponse(HttpStatusCode.OK, "User Profile", "User photo upload successfully",
                    UserApiObject.FromModel(user, profile, geoPoint));
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "User Profile",
                    "Something went wrong trying to uplaod photo", ex);
            }
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


            var profile = await _profileRepo.GetByUserId(user.Id);
            UpdateProfileProps(updateRequestObj, profile);

            
            try
            {
                await UpdateMessageCategorySettings(updateRequestObj, user);
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "User Update Failure",
                    "Something went wrong updating message category settings", ex);
            }
            
            try
            {
                await UpdateMessageDayOfWeekSettings(updateRequestObj, user);
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
            var geoPoint = _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"];
            return CreateResponse(HttpStatusCode.OK, "User Update Success", "User was successfully updated",
                UserApiObject.FromModel(user, profile, geoPoint));
        }

        private async Task UpdateMessageDayOfWeekSettings(UpdateUserObject updateRequestObj, ApplicationUser user)
        {
            var profile = await _profileRepo.GetByUserId(user.Id);
            foreach (var setting in updateRequestObj.MessageDayOfWeekSettings)
            {
                var existingSetting =
                    profile.MessageDayOfWeekSettings.First(s => s.MessageDayOfWeekSettingId == setting.Id);
                existingSetting.Enabled = setting.Enabled;
                existingSetting.StartTime = setting.StartTime;
                existingSetting.EndTime = setting.EndTime;
                existingSetting.NumOfMessages = setting.NumOfMessages;
            }
            await _profileRepo.Update(profile);
        }

        private async Task UpdateMessageCategorySettings(UpdateUserObject updateRequestObj, ApplicationUser user)
        {
            var profile = await _profileRepo.GetByUserId(user.Id);
            foreach (var setting in updateRequestObj.MessageCategorySettings)
                profile.MessageCategorySettings.First(s => s.MessageCategorySettingId == setting.Id).Enabled =
                    setting.Enabled;
            await _profileRepo.Update(profile);
        }

        private static void UpdateProfileProps(UpdateUserObject updateUserObject, ApplicationUserProfile profile)
        {
            profile.FirstName = updateUserObject.FirstName;
            profile.LastName = updateUserObject.LastName;
            profile.CountryCode = updateUserObject.CountryCode;
            profile.PostalCode = updateUserObject.PostalCode;
        }
    }

}
