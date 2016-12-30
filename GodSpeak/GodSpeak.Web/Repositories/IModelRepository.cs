using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GodSpeak.Web.Repositories
{
    public interface IModelRepository<T>
    {
        Task<T> GetById(Guid id);

        Task<List<T>> All();

        void Delete(T model);

        Task Update(T model);
    }
}