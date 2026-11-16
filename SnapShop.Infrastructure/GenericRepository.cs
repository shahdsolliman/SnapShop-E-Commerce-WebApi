using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapShop.Core.Entities;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Infrastructure.Data;
using SnapShop.Core.Specifications;

namespace SnapShop.Infrastructure
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {

        private readonly StoreContext _dbContext;

        public GenericRepository(StoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            // return (IEnumerable<T>) await _dbContext.Set<Product>().Skip(20).Take(10).OrderBy(P=>P.Name).Include(P => P.ProductBrand).ToListAsync();
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T?> GetAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsyncWithSpec(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetEntityWithSpec(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).FirstOrDefaultAsync();
        }

        public async Task<int> GetCountAsync(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).CountAsync();
        }
        private IQueryable<T> ApplySpecifications(ISpecifications<T> spec)
        {
            return SpecificationsEvaluator<T>.GetQuery(_dbContext.Set<T>(), spec);

        }

        public async Task AddAsync(T entity)
            => await _dbContext.Set<T>().AddAsync(entity);

        public void Update(T entity)
            => _dbContext.Set<T>().Update(entity);

        public void Delete(T entity)
            => _dbContext.Set<T>().Remove(entity);
    }
}
