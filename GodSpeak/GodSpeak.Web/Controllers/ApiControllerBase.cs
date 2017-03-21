using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{
    public class ApiControllerBase : ApiController
    {
        private readonly IAuthRepository _authRepository;

        public ApiControllerBase(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        protected HttpResponseMessage CreateResponse(HttpStatusCode responseStatusCode, string responseTitle, string responseMessage)
        {
            return Request.CreateResponse(responseStatusCode, new ApiResponse()
            {
                Title = responseTitle,
                Message = responseMessage
            });
        }

        protected HttpResponseMessage CreateResponse<T>(HttpStatusCode responseStatusCode, string responseTitle, string responseMessage, T payload) where T:class
        {
            return Request.CreateResponse(responseStatusCode, new ApiResponse<T>()
            {
                Title = responseTitle,
                Message = responseMessage,
                Payload = payload
            });
        }

        protected List<string> GetModelErrors()
        {
            return (from state in ModelState from error in state.Value.Errors select error.ErrorMessage).ToList();
        }

        protected HttpResponseMessage CreateMissingTokenResponse()
        {
            return CreateResponse(HttpStatusCode.Forbidden, "Request Failed", "Request is missing required header");
        }

        protected async Task<bool> RequestHasValidAuthToken(HttpRequestMessage request)
        {
            const string tokenKey = "token";
            if (!request.Headers.Contains(tokenKey))
                return false;


            return await _authRepository.UserWithTokenExists(GetAuthToken(request));
        }

        protected static string GetAuthToken(HttpRequestMessage request)
        {
            return request.Headers.GetValues("token").First();
        }

    }
}