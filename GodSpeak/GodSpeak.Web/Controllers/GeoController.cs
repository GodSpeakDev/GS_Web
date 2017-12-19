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

        public GeoController(IInMemoryDataRepository inMemoryDataRepository, IAuthRepository authRepository):base(authRepository)
        {
            _inMemoryDataRepository = inMemoryDataRepository;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse<List<CountryCodeApiObject>>))]
        public HttpResponseMessage Countries()
        {
            if (_countryCodes == null)
            {
                _countryCodes =
                    _inMemoryDataRepository.PostalCodeGeoCache.Values.Select(pc => pc.CountryCode)
                        .Distinct()
                        .Select(c => new CountryCodeApiObject()
                        {
                            Code = c,
                            Title = _inMemoryDataRepository.CountryCodeNameMap[c]
                        }).ToList();
                var noPostalCodes = _inMemoryDataRepository.NoPostalCodeGeoCache.Keys;
                foreach (var entry in noPostalCodes)
                {
                    _countryCodes.Add(new CountryCodeApiObject()
                    {
                        Code = entry,
                        Title = _inMemoryDataRepository.NoPostalCodeGeoCache[entry].Country
                    });
                }
            }


            return CreateResponse(HttpStatusCode.OK, "Success", "Country Codes Retrieved", _countryCodes.OrderBy(c => c.Title));
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        public HttpResponseMessage PostalCodeExists(string countryCode, string postalCode)
        {
            var key = $"{countryCode}-{postalCode}";
            if (!_inMemoryDataRepository.PostalCodeGeoCache.ContainsKey(key))
                return CreateResponse(HttpStatusCode.NotFound, "Postal Code Not Found",
                    $"Postal code {postalCode} does not exist in country {countryCode}");
            var location = _inMemoryDataRepository.PostalCodeGeoCache[key];
            return CreateResponse(HttpStatusCode.OK, "Postal Code Exists",
                $"{location.AdminName1}, {location.AdminName2}");
        }
    }

}
