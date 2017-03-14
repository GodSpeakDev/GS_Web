using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;

namespace GodSpeak.Web.Controllers
{
    public class UserController : ApiController
    {
        private readonly IAuthRepository _authRepository;

        public UserController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost]
        public async Task<string> Login(LoginRequestObject loginRequest)
        {
            var user = await _authRepository.FindUser(loginRequest.Email, loginRequest.Password);
            if (user != null)
                return $"user logged in with {loginRequest.Email} and {loginRequest.Password}";
            else
                return "No luck";
        }
    }

   
}
