using NHibernate;
using NHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NHKit.Callbacks
{
    internal class CallbackHandler
    {
        public Type EntityType { get; }
        // The handler type is currently always the same type as the entity type,
        // but a future enhancement is to allow handlers to be registered in the
        // dependency injection container.
        public Type HandlerType { get; }
        public bool HandlerIsEntity { get;  }
        public MethodInfo InsertedCallbackMethod { get; }
        public Type InsertedtEntityInfoType { get; }
        public MethodInfo UpdatedCallbackMethod { get; }
        public Type UpdatedEntityInfoType { get; }

        private CallbackHandler(Type entityType, Type handlerType)
        {
            EntityType = entityType;
            HandlerType = handlerType;
            HandlerIsEntity = handlerType == entityType;
            InsertedCallbackMethod = GetCallbackMethod(HandlerType, typeof(IHandleEntityInserted<>), EntityType);
            UpdatedCallbackMethod = GetCallbackMethod(HandlerType, typeof(IHandleEntityUpdated<>), EntityType);
            if (InsertedCallbackMethod != null) {
                InsertedtEntityInfoType = typeof(InsertedEntityInfo<>).MakeGenericType(EntityType);
            }

            if (UpdatedCallbackMethod != null) {
                UpdatedEntityInfoType = typeof(UpdatedEntityInfo<>).MakeGenericType(EntityType);
            }
        }

        public object CreateInsertedEntityInfo(ISession session, object entity)
        {
            return Activator.CreateInstance(InsertedtEntityInfoType, session, entity);
        }

        public object CreateUpdatedEntityInfo(ISession session, object entity, object[] oldState, object[] currentState)
        {
            return Activator.CreateInstance(UpdatedEntityInfoType, session, entity, oldState, currentState);
        }

        public static IEnumerable<CallbackHandler> Create(IEnumerable<Type> handlerTypes,
            ICollection<PersistentClass> persistentClasses)
        {
            return handlerTypes.SelectMany(t => Create(t, persistentClasses));
        }

        public static IEnumerable<CallbackHandler> Create(Type handlerType, ICollection<PersistentClass> persistentClasses)
        {
            var entityTypes = GetEntities(handlerType, typeof(IHandleEntityInserted<>), persistentClasses)
                .Concat(GetEntities(handlerType, typeof(IHandleEntityUpdated<>), persistentClasses))
                .Distinct();

            return entityTypes.Select(entityType => new CallbackHandler(handlerType, entityType));
        }

        private static IEnumerable<Type> GetEntities(Type handlerType, Type genericType, ICollection<PersistentClass> persistentClasses)
        {
            var persistentClass = persistentClasses.FirstOrDefault(pc => pc.MappedClass == handlerType);
            var entityTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType)
                .Select(i => i.GenericTypeArguments.First())
                .Where(t => persistentClasses.Any(pc => pc.MappedClass == t))
                .ToList();

            if (entityTypes.Count == 0) {
                return System.Array.Empty<Type>();
            }

            if (persistentClass != null) {
                if (entityTypes.Count > 1) {
                    throw new InvalidOperationException(
                        $"Class {handlerType.FullName} has multiple {genericType} interfaces");
                }
                var entityType = entityTypes.First();
                if (entityType != persistentClass.MappedClass) {
                    throw new InvalidOperationException(
                        $"Class {persistentClass.MappedClass.FullName} has {genericType} interface but the type does not match");
                }
            }
            return entityTypes;
        }

        private static MethodInfo GetCallbackMethod(Type handlerType, Type genericType, Type entityType)
        {
            var interfaceType = handlerType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType)
                .FirstOrDefault(x => x.GenericTypeArguments[0] == entityType);

            return interfaceType?.GetMethods().FirstOrDefault();
        }
    }
}
