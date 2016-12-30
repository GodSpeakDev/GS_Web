using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{
    public interface IApplicationUserInviteRepository:IModelRepository<ApplicationUserInvite>
    {
        ApplicationUserInvite GetByCode(string id);
    }
}