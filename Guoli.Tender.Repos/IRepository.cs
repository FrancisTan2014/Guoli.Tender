using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Guoli.Tender.Repos
{
    public interface IRepository
    {
        ICollection<object> GetAll();
        ICollection<object> Find(Expression<Func<object, bool>> predicate);
        object Get(object id);
        object Insert(object model);
        bool Update(object model);
        bool Remove(object id);
    }

    public interface IRepository<TEntity, in TKey>
        where TEntity: class
    {
        ICollection<TEntity> GetAll();
        ICollection<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity Get(TKey id);
        TEntity Insert(TEntity model);
        void BulkInsert(IEnumerable<TEntity> models);
        bool Update(TEntity model);
        bool Remove(TKey id);
    }
}
