using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapShop.Core.Entities;
using SnapShop.Core.Specifications;

namespace SnapShop.Core.Repositories.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetEntityWithSpec(ISpecifications<T> spec);
        Task<IReadOnlyList<T>> GetAllAsyncWithSpec(ISpecifications<T> spec);
        Task<T?> GetAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<int> GetCountAsync(ISpecifications<T> spec);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
