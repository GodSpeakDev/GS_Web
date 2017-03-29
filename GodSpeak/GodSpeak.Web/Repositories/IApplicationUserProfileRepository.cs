using System.Threading.Tasks;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{
    public interface IApplicationUserProfileRepository:IModelRepository<ApplicationUserProfile>
    {
        Task<ApplicationUserProfile> GetByCode(string id);

        Task<ApplicationUserProfile> GetByUserId(string id);

        Task<bool> ApplyInviteCredit(string userId, int numOfInvites);
    }
}