using System;
using System.Collections.Generic;

namespace GodSpeak.Web.Repositories
{
    public interface IModelRepository<T>
    {
        T GetById(Guid id);

        List<T> All();

        void Delete(T model);

        void Update(T model);
    }
}