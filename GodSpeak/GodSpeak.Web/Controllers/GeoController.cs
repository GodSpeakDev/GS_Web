using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Http.Description;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{
    [Route("api/geo/{action}")]
    public class GeoController : ApiControllerBase
    {
        private readonly IInMemoryDataRepository _inMemoryDataRepository;

        private static List<CountryCodeApiObject> _countryCodes;

        public GeoController(IInMemoryDataRepository inMemoryDataRepository)
        {
            _inMemoryDataRepository = inMemoryDataRepository;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<List<CountryCodeApiObject>>))]
        public HttpResponseMessage Countries()
        {
            if (_countryCodes == null)
                _countryCodes =
                    _inMemoryDataRepository.PostalCodeGeoCache.Values.Select(pc => pc.CountryCode)
                        .Distinct()
                        .Select(c => new CountryCodeApiObject()
                        {
                            Code = c,
                            Title = _inMemoryDataRepository.CountryCodeNameMap[c]
                        }).ToList();


            return CreateResponse(HttpStatusCode.OK, "Success", "Country Codes Retrieved", _countryCodes);
        }
    }

    
}
