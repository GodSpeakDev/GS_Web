using System.Threading.Tasks;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{
    public interface IApplicationUserProfileRepository:IModelRepository<ApplicationUserProfile>
    {
        Task<ApplicationUserProfile> GetByCode(string id);
    }
}