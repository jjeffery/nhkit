using NHibernate;
using System;
using System.Linq.Expressions;

namespace NHKit.Callbacks
{
    public interface IHandleEntityInserted<in TEntity>
    {
        void OnEntityInserted(IInsertedEntityInfo<TEntity> entity);
    }

    public interface IInsertedEntityInfo<out TEntity>
    {
        ISession Session { get; }
        TEntity Entity { get; }
    }

    public interface IHandleEntityUpdated<TEntity>
    {
        void OnEntityUpdated(IUpdatedEntityInfo<TEntity> entity);
    }

    public interface IUpdatedEntityInfo<TEntity>
    {
        ISession Session { get; }
        TEntity Entity { get; }
        bool HasPropertyChanged<TProperty>(Expression<Func<TEntity, TProperty>> expression);
        TProperty GetOldValue<TProperty>(Expression<Func<TEntity, TProperty>> expression);
        TProperty GetCurrentValue<TProperty>(Expression<Func<TEntity, TProperty>> expression);
    }
}
