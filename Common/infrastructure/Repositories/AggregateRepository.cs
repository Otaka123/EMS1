//using Common.Application.Contracts.interfaces;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Common.infrastructure.Repositories
//{
//    public class AggregateRepository<T> : Repository<T>, IAggregateRepository<T>
//      where T : class, IAggregateRoot
//    {
//        public AggregateRepository(DbContext context) : base(context) { }

//        //public async Task<T> GetByIdWithAllAsync<TId>(TId id, CancellationToken cancellationToken = default)
//        //    where TId : notnull
//        //{
//        //    var entity = await _dbSet.FindAsync(new object[] { id! }, cancellationToken);

//        //    if (entity != null)
//        //    {
//        //        // تحميل كل العلاقات للتجمع
//        //        foreach (var navigation in _context.Model
//        //            .FindEntityType(typeof(T))!
//        //            .GetNavigations())
//        //        {
//        //            await _context.Entry(entity).Collection(navigation.Name).LoadAsync(cancellationToken);
//        //        }
//        //    }

//        //    return entity;
//        //}
//        public async Task<T?> GetByIdWithAllAsync<TId>(TId id, CancellationToken cancellationToken = default)
//    where TId : notnull
//        {
//            var entity = await _dbSet.FindAsync(new object[] { id! }, cancellationToken);

//            if (entity != null)
//            {
//                foreach (var navigation in _context.Model
//                    .FindEntityType(typeof(T))!
//                    .GetNavigations())
//                {
//                    await _context.Entry(entity).Collection(navigation.Name).LoadAsync(cancellationToken);
//                }
//            }

//            return entity;
//        }
//        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
//    => await _dbSet.ToListAsync(cancellationToken);

//        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
//            => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

//        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
//            => await _dbSet.AddRangeAsync(entities, cancellationToken);

//        public void RemoveRange(IEnumerable<T> entities)
//            => _dbSet.RemoveRange(entities);

//        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
//            => await _dbSet.AnyAsync(predicate);

//        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
//            => await _dbSet.CountAsync(predicate, cancellationToken);
//    }
//}
