using SnapShop.Core;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _dbContext;
        private Dictionary<string, object> _repositories;
        
        public UnitOfWork(StoreContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Dictionary<string, object>();
        }
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            var key = typeof(TEntity).Name;
            if(! _repositories.ContainsKey(key))
            {
                var repository = new GenericRepository<TEntity>(_dbContext) /*as GenericRepository<BaseEntity>*/;
                _repositories.Add(key, repository);  // warning because [as] is explicitly casted
            }
            return (IGenericRepository<TEntity>)_repositories[key];
        }

        public async Task<int> CompleteAsync()
            => await _dbContext.SaveChangesAsync();

        public async ValueTask DisposeAsync()
            => await _dbContext.DisposeAsync();

    }
}
