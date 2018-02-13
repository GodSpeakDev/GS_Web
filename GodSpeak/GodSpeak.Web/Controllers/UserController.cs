using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
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
        private readonly IAppShareRepository _appShareRepo;
        private readonly IInviteRepository _inviteRepository;

        public UserController(IApplicationUserProfileRepository profileRepo, IAuthRepository authRepository, ApplicationUserManager userManager,
            UserRegistrationUtil regUtil, IInMemoryDataRepository inMemoryDataRepo, IIdentityMessageService messageService, IImpactRepository impactRepository, IAppShareRepository appShareRepository, IInviteRepository inviteRepository) :base(authRepository)
        {
            var provider = new DpapiDataProtectionProvider("Sample");

            _profileRepo = profileRepo;
            _authRepository = authRepository;
            _userManager = userManager;
            _userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("EmailConfirmation"));
            _userManager.UserValidator = new UserValidator<ApplicationUser>(_userManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };

            _regUtil = regUtil;
            _inMemoryDataRepo = inMemoryDataRepo;
            _messageService = messageService;
            _impactRepository = impactRepository;
            _appShareRepo = appShareRepository;
            _inviteRepository = inviteRepository;

            
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


            if (await _userManager.FindByEmailAsync(emailAddress) == null)
            {
                return CreateResponse(HttpStatusCode.OK, "Recover Password Failed",
                "We do not have any records of that email address");
            }
            
            var id = (await _userManager.Users.FirstAsync(u => u.Email == emailAddress)).Id;
            
            var newPassword = _regUtil.GenerateInviteCode();
            _userManager.RemovePassword(id);
            _userManager.AddPassword(id, newPassword);
//            await _userManager.ResetPasswordAsync(id, token, newPassword);
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
            
            if (registerUserObject.Platform == "android" && string.IsNullOrEmpty(registerUserObject.InviteCode))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "Request is missing ReferringInviteCode");

            if (registerUserObject.Platform == "android" && await _profileRepo.GetByCode(registerUserObject.InviteCode) == null)
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "Request's invite code is invalid");

            if (registerUserObject.Platform == "android" && (await _profileRepo.GetByCode(registerUserObject.InviteCode)).InviteBalance == 0)
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "The referring invite code does not have anymore available invites.");


            if (registerUserObject.Password != registerUserObject.PasswordConfirm)
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "The submitted passwords do not match");
            

            if (await _userManager.Users.AnyAsync(u => u.Email == registerUserObject.EmailAddress))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "User with submitted email already exists");

            if (
                !_inMemoryDataRepo.PostalCodeGeoCache.ContainsKey(
                    $"{registerUserObject.CountryCode}-{registerUserObject.PostalCode}")
                    && !_inMemoryDataRepo.NoPostalCodeGeoCache.ContainsKey(registerUserObject.CountryCode))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "Country and/or Postal code is invalid.");

            try
            {
                var result = _userManager.Create(new ApplicationUser()
                {
                    Email = registerUserObject.EmailAddress,
                    UserName = registerUserObject.EmailAddress
                }, registerUserObject.Password);

                if (result.Errors.Any())
                    return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure", result.Errors.First());

                Debug.Write("Testing...");
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "Registration Failure",
                    "Something went wrong trying create user security record.", ex);
            }

            var allUsers = await _userManager.Users.ToListAsync();
            
            var user = await _userManager.Users.FirstAsync(u => u.Email == registerUserObject.EmailAddress);

            var referringEmailAddress = "";
            var referringAddresses = new List<string>();

            if (!string.IsNullOrEmpty(registerUserObject.InviteCode))
            {
                var referringUserId = (await _profileRepo.GetByCode(registerUserObject.InviteCode)).UserId;
                referringEmailAddress = (await _userManager.FindByIdAsync(referringUserId)).Email;
                referringAddresses = await _regUtil.GetParentEmailAddresses(referringEmailAddress);
                referringAddresses.Add(referringEmailAddress);
            }
            else
            {
                referringAddresses = await _appShareRepo.GetReferrals(user.Email);
                if (referringAddresses.Any())
                    referringEmailAddress = referringAddresses.First();

            }

            var inviteCode = _regUtil.GenerateInviteCode();
            while (await _profileRepo.GetByCode(inviteCode) != null)
                inviteCode = _regUtil.GenerateInviteCode();


            var profile = new ApplicationUserProfile
            {
                MessageCategorySettings = _regUtil.GenerateDefaultMessageCategorySettings(),
                MessageDayOfWeekSettings = _regUtil.GenerateDefaultDayOfWeekSettingsForUser(),
                Code = inviteCode,
                FirstName = registerUserObject.FirstName,
                LastName = registerUserObject.LastName,
                CountryCode = registerUserObject.CountryCode,
                PostalCode = registerUserObject.PostalCode,
                UserId = user.Id,
                Token = _authRepository.CreateToken(),
                ReferringEmailAddress = referringEmailAddress,
                InviteBalance = 0,
                DateRegistered = DateTime.Now
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

            if (registerUserObject.Platform == "android")
            {
                try
                {
                    var referringUser = await _profileRepo.GetByCode(registerUserObject.InviteCode);
                    referringUser.InviteBalance = Math.Max(0, referringUser.InviteBalance - 1);
                    await _profileRepo.Update(referringUser);
                }
                catch (Exception ex)
                {
                    return CreateResponse(HttpStatusCode.InternalServerError, "Registration Failure",
                        "Something went wrong trying to decriment the referring users balance.", ex);
                }
            }

            try
            {
                if (referringAddresses.Any())
                    foreach (var address in referringAddresses)
                        await _impactRepository.RecordImpact(DateTime.Now, registerUserObject.PostalCode,
                            registerUserObject.CountryCode, address);
            }
            catch (Exception impactException)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "Registration Impact Failure",
                    "Something went wrong trying to record impact.", impactException);
            }




            var geoPoint =
                (_inMemoryDataRepo.PostalCodeGeoCache.ContainsKey($"{profile.CountryCode}-{profile.PostalCode}"))
                    ? _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"]
                    : new PostalCodeGeoLocation()
                    {
                        Latitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Latitude,
                        Longitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Longitude,
                    };
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
            var user = await _userManager.FindByIdAsync(userId);
            var profile = await _profileRepo.GetByUserId(userId);

            profile.ReferringEmailAddress = referralObj.ReferringEmailAddress;

            try
            {
                await _profileRepo.Update(profile);
            }
            catch (Exception profileUpdatException)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "Referral Registration Failure",
                   "Something went wrong trying to update user profile.", profileUpdatException);
            }

            try
            {
                await
                    _impactRepository.RecordImpact(DateTime.Now, profile.PostalCode, profile.CountryCode,
                        referralObj.ReferringEmailAddress);
            }
            catch (Exception impactException)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "Referral Registration Failure",
                   "Something went wrong trying to record impact.", impactException);
            }

            return CreateResponse(HttpStatusCode.OK, "User", "Referral has been registered");
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        [Route("api/User/Impact")]
        public async Task<HttpResponseMessage> Impact()
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            
            var days = (await _impactRepository.GetImpactForUserId(userId)).ToList().Select(ImpactApiObject.FromModel).ToList();

          

            return CreateResponse(HttpStatusCode.OK, "Impact", $"Impact for code {userId}",
                days.OrderBy(d => d.Date));

        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [Route("api/User")]
        public async Task<HttpResponseMessage> Profile()
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            var user = await _userManager.Users.FirstAsync(u => u.Id == userId);

            var profile = await _profileRepo.GetByUserId(user.Id);
            var geoPoint =
                (_inMemoryDataRepo.PostalCodeGeoCache.ContainsKey($"{profile.CountryCode}-{profile.PostalCode}"))
                    ? _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"]
                    : new PostalCodeGeoLocation()
                    {
                        Latitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Latitude,
                        Longitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Longitude,
                    };
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



                var geoPoint =
                (_inMemoryDataRepo.PostalCodeGeoCache.ContainsKey($"{profile.CountryCode}-{profile.PostalCode}"))
                    ? _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"]
                    : new PostalCodeGeoLocation()
                    {
                        Latitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Latitude,
                        Longitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Longitude,
                    };


                return CreateResponse(HttpStatusCode.OK, "User Profile", "User photo upload successfully",
                    UserApiObject.FromModel(user, profile, geoPoint));
            }
            catch (Exception ex)
            {
                return CreateResponse(HttpStatusCode.InternalServerError, "User Profile",
                    "Something went wrong trying to upload photo", ex);
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
            if (
                !_inMemoryDataRepo.PostalCodeGeoCache.ContainsKey(
                    $"{updateRequestObj.CountryCode}-{updateRequestObj.PostalCode}")
                    && !_inMemoryDataRepo.NoPostalCodeGeoCache.ContainsKey(updateRequestObj.CountryCode))
                return CreateResponse(HttpStatusCode.BadRequest, "Registration Failure",
                    "Country and/or Postal code is invalid.");


            var userId = await _authRepository.GetUserIdForToken(GetAuthToken(Request));
            var user = await _userManager.Users.FirstAsync(u => u.Id == userId);

            if (!string.IsNullOrEmpty(updateRequestObj.ReferringEmailAddress))
            {
                //making sure they haven't specified their own email as the referring email address
                if(String.Equals(updateRequestObj.ReferringEmailAddress, user.Email, StringComparison.CurrentCultureIgnoreCase))
                    return CreateResponse(HttpStatusCode.BadRequest, "User Update Failure",
                                   "Referring email address cannot be the same as user's email address.");

                //making sure there's a user record for referring email address
                var referringUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == updateRequestObj.ReferringEmailAddress);
                if (referringUser == null)
                    return CreateResponse(HttpStatusCode.BadRequest, "User Update Failure",
                                   "Referring email address does not match any existing users.");

                //making sure the two users haven't referenced each other
                var referringUserProfile = await _profileRepo.GetByUserId(referringUser.Id);
                if (String.Equals(referringUserProfile.ReferringEmailAddress, user.Email, StringComparison.CurrentCultureIgnoreCase))
                {
                    return CreateResponse(HttpStatusCode.BadRequest, "User Update Failure",
                                   "Referring email address has you listed as their referring email address.");
                }
            }

            //update password if needed
            if (!string.IsNullOrEmpty(updateRequestObj.CurrentPassword) && await _authRepository.FindUser(user.UserName, updateRequestObj.CurrentPassword) == null)
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
            var geoPoint =
                (_inMemoryDataRepo.PostalCodeGeoCache.ContainsKey($"{profile.CountryCode}-{profile.PostalCode}"))
                    ? _inMemoryDataRepo.PostalCodeGeoCache[$"{profile.CountryCode}-{profile.PostalCode}"]
                    : new PostalCodeGeoLocation()
                    {
                        Latitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Latitude,
                        Longitude = _inMemoryDataRepo.NoPostalCodeGeoCache[profile.CountryCode].Longitude,
                    };
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
            profile.ReferringEmailAddress = updateUserObject.ReferringEmailAddress;
        }
    }

}
