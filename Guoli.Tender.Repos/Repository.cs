﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Guoli.Tender.Repos
{
    public abstract class Repository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly DbContext _dbContext;

        protected Repository()
            : this(Activator.CreateInstance<TContext>())
        {

        }

        protected Repository(DbContext context)
        {
            _dbContext = context;
        }

        public virtual bool Remove(TKey id)
        {
            var model = Get(id);
            if (model == null)
            {
                return false;
            }

            GetSet().Remove(model);
            return _dbContext.SaveChanges() > 0;
        }

        public ICollection<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return GetSet().Where(predicate).ToList();
        }

        public TEntity Get(TKey id)
        {
            return GetSet().Find(id);
        }

        public ICollection<TEntity> GetAll()
        {
            return GetSet().ToList();
        }

        public TEntity Insert(TEntity model)
        {
            GetSet().Add(model);
            _dbContext.SaveChanges();
            return model;
        }

        public bool Update(TEntity model)
        {
            GetSet().Attach(model);
            _dbContext.Entry(model).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        protected DbSet<TEntity> GetSet()
        {
            return _dbContext.Set<TEntity>();
        }
    }
}