using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GodSpeak.Web.Repositories
{
    public interface IModelRepository<T>:IDisposable
    {
        Task<T> GetById(Guid? id);

        Task<List<T>> All();

        Task Delete(T model);

        Task Update(T model);

        Task Insert(T model);
    }
}