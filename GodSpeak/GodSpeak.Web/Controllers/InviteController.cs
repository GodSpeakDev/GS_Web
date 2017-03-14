using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{
    public class InviteController : ApiController
    {
        private readonly IInviteRepository _inviteRepository;

        public InviteController(IInviteRepository inviteRepository)
        {
            _inviteRepository = inviteRepository;
        }

        [HttpGet]
        public async Task<bool> Validate(string inviteCode)
        {
            return await _inviteRepository.InviteCodeIsValid(inviteCode) &&
                 await _inviteRepository.InviteCodeHasBalance(inviteCode);
        }
    }
}
