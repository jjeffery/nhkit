using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NHKit.Callbacks
{
    internal abstract class UpdatedEntityInfoBase
    {
        public ISession Session { get; }
        protected object EntityObject { get; }
        private readonly string _entityName;
        protected readonly object[] OldState;
        protected readonly object[] CurrentState;
        private readonly string[] _propertyNames;
        private readonly int[] _dirtyProps;

        protected UpdatedEntityInfoBase(ISession session, object entity, object[] oldState, object[] currentState)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            EntityObject = entity ?? throw new ArgumentNullException(nameof(entity));
            OldState = oldState ?? throw new ArgumentNullException(nameof(oldState));
            CurrentState = currentState ?? throw new ArgumentNullException(nameof(currentState));
            ISessionImplementor sessionImpl = session.GetSessionImplementation();
            IPersistenceContext persistenceContext = sessionImpl.PersistenceContext;
            EntityEntry entry = sessionImpl.PersistenceContext.GetEntry(entity);
            if (entry == null) {
                if (entity is INHibernateProxy proxy) {
                    var obj = persistenceContext.Unproxy(proxy);
                    entry = persistenceContext.GetEntry(obj);
                }

                if (entry == null) {
                    throw new ArgumentException("Entity is not in the session", nameof(entity));
                }
            }
            _entityName = entry.EntityName;
            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(entry.EntityName);
            _propertyNames = persister.PropertyNames;
            _dirtyProps = persister.FindDirty(CurrentState, OldState, entity, sessionImpl) ?? Array.Empty<int>();
        }

        public bool HasPropertyChanged(string propertyName) => HasPropertyChanged(GetPropertyIndex(propertyName));
        public object GetCurrentValue(string propertyName) => CurrentState[GetPropertyIndex(propertyName)];
        public object GetLoadedValue(string propertyName) => OldState[GetPropertyIndex(propertyName)];

        protected bool HasPropertyChanged(int propertyIndex) => Array.IndexOf(_dirtyProps, propertyIndex) >= 0;

        protected int GetPropertyIndex(string propertyName)
        {
            var index = Array.IndexOf(_propertyNames, propertyName);
            if (index < 0) {
                throw new ArgumentException($"Property '{propertyName}' does not exist on entity '{_entityName}'");
            }

            return index;
        }

        protected string GetPropertyName(int propertyIndex)
        {
            return _propertyNames[propertyIndex];
        }
    }

    internal sealed class UpdatedEntityInfo : UpdatedEntityInfoBase
    {
        public object Entity => EntityObject;
        public UpdatedEntityInfo(ISession session, object entity, object[] oldState, object[] currentState)
            : base(session, entity, oldState, currentState) { }
    }

    internal sealed class UpdatedEntityInfo<TEntity> : UpdatedEntityInfoBase, IUpdatedEntityInfo<TEntity> where TEntity : class
    {
        public UpdatedEntityInfo(ISession session, TEntity entity, object[] oldState, object[] currentState)
            : base(session, entity, oldState, currentState) { }

        public TEntity Entity => (TEntity)EntityObject;

        public bool HasPropertyChanged<TProperty>(Expression<Func<TEntity, TProperty>> expression)
            => HasPropertyChanged(GetPropertyIndex(expression));

        public TProperty GetOldValue<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            return (TProperty)OldState[GetPropertyIndex(expression)];
        }

        public TProperty GetCurrentValue<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            return (TProperty)CurrentState[GetPropertyIndex(expression)];
        }

        private int GetPropertyIndex<TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            var properties = GetProperties(expression.Body);
            if (properties.Count > 1) {
                throw new ArgumentException("Expression reference direct property, not a member of a component");
            }
            return GetPropertyIndex(properties[0].Name);
        }

        private static List<PropertyInfo> GetProperties(Expression expression)
        {
            var properties = new List<PropertyInfo>();
            if (expression is MemberExpression memberExpression) {
                while (memberExpression != null) {
                    if (memberExpression.Member is PropertyInfo property) {
                        properties.Add(property);
                    }

                    memberExpression = memberExpression.Expression as MemberExpression;
                }
            }

            if (properties.Count == 0) {
                throw new ArgumentException("Expression must be a property");
            }
            properties.Reverse();
            return properties;
        }
    }
}
