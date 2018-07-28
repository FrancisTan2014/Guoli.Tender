using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Guoli.Tender.Repos
{
    public interface IRepository<TEntity, in TKey>
        where TEntity: class
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity Get(TKey id);
        TEntity Insert(TEntity model);
        void BulkInsert(IEnumerable<TEntity> models);
        bool Update(TEntity model);
        bool Remove(TKey id);
    }
}
