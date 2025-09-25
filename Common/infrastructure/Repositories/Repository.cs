//using Common.Application.Contracts.interfaces;
//using Microsoft.EntityFrameworkCore;
//using Polly;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Common.infrastructure.Repositories
//{
//    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
//    {
//        protected readonly DbContext _context;
//        protected readonly DbSet<TEntity> _dbSet;

//        public Repository(DbContext context)
//        {
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//            _dbSet = _context.Set<TEntity>();
//        }

//        public async Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
//        {
//            return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
//        }

//        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
//        {
//            return await _dbSet.ToListAsync(cancellationToken);
//        }

//        public async Task<IEnumerable<TEntity>> FindAsync(
//            Expression<Func<TEntity, bool>> predicate,
//            CancellationToken cancellationToken = default)
//        {
//            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
//        }

//        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
//        {
//            await _dbSet.AddAsync(entity, cancellationToken);
//        }

//        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
//        {
//            await _dbSet.AddRangeAsync(entities, cancellationToken);
//        }

//        public void Update(TEntity entity)
//        {
//            _dbSet.Update(entity);
//        }

//        public void Remove(TEntity entity)
//        {
//            _dbSet.Remove(entity);
//        }

//        public void RemoveRange(IEnumerable<TEntity> entities)
//        {
//            _dbSet.RemoveRange(entities);
//        }

//        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
//        {
//            return await _dbSet.AnyAsync(predicate);
//        }

//        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate,
//            CancellationToken cancellationToken = default)
//        {
//            return await _dbSet.CountAsync(predicate, cancellationToken);
//        }

//        public IQueryable<TEntity> Query()
//        {
//            return _dbSet.AsQueryable();
//        }
//    }
//}
