using System;
using System.Collections.Generic;
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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;

namespace GodSpeak.Web.Controllers
{
    public class UserController : ApiControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public UserController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

      

        [HttpPost]
        [ResponseType(typeof(ApiResponse<UserApiObject>))]
        public async Task<HttpResponseMessage> Login(LoginApiObject loginApi)
        {
            var user = await _authRepository.FindUser(loginApi.Email, loginApi.Password);
            if (user == null)
                return CreateResponse(HttpStatusCode.Forbidden, "Login Invalid", "Submitted credentials are invalid");
            return CreateResponse(HttpStatusCode.OK, "Login Valid", "Submitted credentials were valid", UserApiObject.FromModel((ApplicationUser)user));
        }

        
    }

   
}
