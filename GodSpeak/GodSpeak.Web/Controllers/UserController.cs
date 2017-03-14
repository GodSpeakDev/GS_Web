using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Controllers
{
    public class UserController : ApiController
    {
        [HttpPost]
        public string Login(LoginRequestObject loginRequest)
        {
            return $"user logged in with {loginRequest.Email} and {loginRequest.Password}";
        }
    }

   
}
