using System.Threading.Tasks;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{
    public interface IApplicationUserInviteRepository:IModelRepository<ApplicationUserInvite>
    {
        Task<ApplicationUserInvite> GetByCode(string id);
    }
}