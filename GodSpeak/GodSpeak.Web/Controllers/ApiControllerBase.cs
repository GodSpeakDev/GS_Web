using System.Net;
using System.Net.Http;
using System.Web.Http;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Controllers
{
    public class ApiControllerBase : ApiController
    {
        protected HttpResponseMessage CreateResponse(HttpStatusCode responseStatusCode, string responseTitle, string responseMessage)
        {
            return Request.CreateResponse(responseStatusCode, new ApiResponse()
            {
                Title = responseTitle,
                Message = responseMessage
            });
        }
    }
}