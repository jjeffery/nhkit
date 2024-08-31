using JetBrains.Annotations;
using NHibernate;
using NHibernate.Proxy;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NHKit.Testing.Factories;

    public abstract class Factory
    {
        public static readonly IDictionary<Type, Type> Builders = new Dictionary<Type, Type>();
        private static readonly Type BuilderInterfaceType = typeof(IFactory<>);
        private static readonly ConditionalWeakTable<TestDatabase, Dictionary<Factory, object>> TestDatabases = new();

        public Type Type { get; }

        protected Factory([NotNull] Type type)
        {
            Type = type;
        }

        static Factory()
        {
            var assembly = typeof(Factory).Assembly;

            foreach (var type in assembly.GetTypes()) {
                if (type.IsAbstract) {
                    continue;
                }

                if (!type.IsPublic) {
                    continue;
                }

                foreach (var interfaceType in type.GetInterfaces()) {
                    if (!interfaceType.IsGenericType) {
                        continue;
                    }

                    if (interfaceType.GetGenericTypeDefinition() != BuilderInterfaceType) {
                        continue;
                    }

                    var builtType = interfaceType.GenericTypeArguments[0];
                    Builders.Add(builtType, type);
                }
            }
        }

        public static bool ExistsForModel(Type modelType)
        {
            return Builders.ContainsKey(modelType);
        }

        public static Factory Create(Type modelType)
        {
            if (!Builders.TryGetValue(modelType, out var builderType)) {
                throw new NotImplementedException($"No factory exists for type {modelType}");
            }
            var builder = (Factory)Activator.CreateInstance(builderType);
            return builder;
        }

        protected abstract object CreateInstanceObject(ISession session);
        protected abstract void DoAfterSave(ISession session, object instance);
        
        /// <summary>
        /// Returns the instance for the session, creating it if necessary.
        /// </summary>
        public object Get([NotNull] ISession session)
        {
            var testDatabase = TestDatabase.FromSession(session);
            var ids = TestDatabases.GetOrCreateValue(testDatabase);
            if (ids.TryGetValue(this, out var id)) {
                return session.Get(Type, id);
            }

            var entity = CreateInstanceObject(session);
            if (entity == null) {
                throw new ApplicationException($"{nameof(CreateInstanceObject)} returned null");
            }

            if (!Type.IsInstanceOfType(entity)) {
                throw new ApplicationException(
                    $"{nameof(CreateInstanceObject)} did not return an instance of type {Type.FullName}");
            }

            var entityPersister = session.GetSessionImplementation()
                .GetEntityPersister(NHibernateProxyHelper.GuessClass(entity).FullName, entity);
            id = entityPersister.GetIdentifier(entity);
            ids.Add(this, id);
            session.Flush();
            DoAfterSave(session, entity);
            if (session.IsDirty()) {
                // Flush again just because the entity was modified in the after save callbacks 
                session.Flush();
            }
            // Refresh to populate any collection properties.
            session.Refresh(entity);
            return entity;
        }
        
        public Factory AddTo(IList<Factory> list)
        {
            list.Add(this);
            return this;
        }
        
        /// <summary>
        /// Ensure that the instance is created in the database associated with the session.
        /// </summary>
        public Factory Save([NotNull] ISession session)
        {
            Get(session);
            return this;
        }

        public static void ClearAll(TestDatabase testDatabase)
        {
            TestDatabases.Remove(testDatabase);
        }
    }
    
    public abstract class Factory<TModel> : Factory, IFactory<TModel> where TModel : class {
        protected Factory() : base(typeof(TModel)) { }

        /// <summary>
        /// Returns the instance for the session, creating it if necessary.
        /// </summary>
        public new TModel Get(ISession session)
        {
            return (TModel) base.Get(session);
        }
        
        public new Factory<TModel> AddTo(IList<Factory> list)
        {
            list.Add(this);
            return this;
        }
    }

    public abstract class Factory<TModel, TFactory> : Factory<TModel> where TModel : class where TFactory : Factory<TModel, TFactory>
    {
        public DateTime Now = DateTime.Now;
        public DateTime Today = DateTime.Today;
        // ReSharper disable once StaticMemberInGenericType
        private static int _nextNumber;
        private readonly List<Action<ISession, TModel>> _withActions = new List<Action<ISession, TModel>>();
        private readonly List<Action<ISession, TModel>> _actions = new List<Action<ISession, TModel>>();
        private readonly List<Action<ISession, TModel>> _afterSaveActions = new List<Action<ISession, TModel>>();

        protected abstract TModel CreateInstance(ISession session);

        protected sealed override object CreateInstanceObject(ISession session)
        {
            return CreateInstance(session);
        }

        protected sealed override void DoAfterSave(ISession session, object instance)
        {
            var typedInstance = (TModel) instance;

            void Refresh()
            {
                if (_afterSaveActions.Count > 0) {
                    // Because the instance has just been created, if it has any
                    // collection properties, they will not be accurate, and in fact
                    // may be null. Before we pass this instance to the callback,
                    // refresh the object to populate any of these collections.
                    session.Refresh(instance);
                }
            }

            Refresh();
            foreach (var action in _afterSaveActions) {
                action(session, typedInstance);
            }
            _afterSaveActions.Clear();
        }

        protected static string PhoneNumber(string prefix, int number)
        {
            var phone = $"{prefix}{number}";
            while (phone.Length < 10) {
                phone += "0";
            }

            return phone;
        }

        public TFactory With<TProperty>(Expression<Func<TModel, TProperty>> expr, Factory<TProperty> factory = null) where TProperty : class
        {
            if (expr.Body is MemberExpression memberExpression) {
                if (memberExpression.Member is PropertyInfo property) {
                    if (factory == null) {
                        factory = (Factory<TProperty>)Factory.Create(typeof(TProperty));
                    }
                    _withActions.Add((session, instance) => SetAssociation(session, instance, property, factory));
                    return (TFactory)this;
                }
            }
            throw new ArgumentException("Expression must be a property");
        }

        protected void Association<TProperty>(Expression<Func<TModel, TProperty>> expr, Factory<TProperty> factory = null) where TProperty : class
        {
            if (expr.Body is MemberExpression memberExpression) {
                if (memberExpression.Member is PropertyInfo property) {
                    if (factory == null) {
                        factory = (Factory<TProperty>) Factory.Create(typeof(TProperty));
                    }
                    _actions.Add((session, instance) => SetAssociation(session, instance, property, factory));
                    return;
                }
            }
            throw new ArgumentException("Expression must be a property");
        }

        protected void Association<TProperty>(Expression<Func<TModel, TProperty>> expr, Func<TModel, TProperty> value) where TProperty : class
        {
            if (expr.Body is MemberExpression memberExpression) {
                if (memberExpression.Member is PropertyInfo property) {
                    _actions.Add((session, instance) => SetAssociation(session, instance, property, value));
                    return;
                }
            }
            throw new ArgumentException("Expression must be a property");
        }
        
        protected void Property<TProperty>(Expression<Func<TModel, TProperty>> expr, Func<ISession, TModel, TProperty> func)
        {
            if (expr.Body is MemberExpression memberExpression) {
                if (memberExpression.Member is PropertyInfo property) {
                    _actions.Add((session, instance) => SetProperty(session, instance, property, func));
                    return;
                }
            }
            throw new ArgumentException("Expression must be a property");
        }

        protected void Property<TProperty>(Expression<Func<TModel, TProperty>> expr, Func<TModel, TProperty> func)
        {
            if (expr.Body is MemberExpression memberExpression) {
                if (memberExpression.Member is PropertyInfo property) {
                    TProperty Func(ISession session, TModel instance) => func(instance);
                    _actions.Add((session, instance) => SetProperty(session, instance, property, Func));
                    return;
                }
            }
            throw new ArgumentException("Expression must be a property");
        }

        private void SetAssociation(ISession session, TModel instance, PropertyInfo property, Factory factory)
        {
            var value = property.GetValue(instance);
            if (value == null) {
                value = factory.Get(session);
                property.SetValue(instance, value);
            }
        }

        private void SetAssociation(ISession session, TModel instance, PropertyInfo property, Func<TModel, object> func)
        {
            var value = property.GetValue(instance);
            if (value == null) {
                value = func(instance);
                property.SetValue(instance, value);
            }
        }

        private void SetProperty<TProperty>(ISession session, TModel instance, PropertyInfo property, Func<ISession, TModel, TProperty> func)
        {
            var value = func(session, instance);
            property.SetValue(instance, value);
        }

        protected void PerformActions(ISession session, TModel instance)
        {
            // with actions have priority
            foreach (var action in _withActions) {
                action(session, instance);
            }
            foreach (var action in _actions) {
                action(session, instance);
            }

            _actions.Clear();
        }

        protected int NextNumber()
        {
            return ++_nextNumber;
        }

        /// <summary>
        /// Ensure that the instance is created in the database associated with the session.
        /// </summary>
        public new TFactory Save(ISession session)
        {
            base.Get(session);
            return (TFactory)this;
        }

        public TFactory AfterSave(Action<ISession, TModel> action)
        {
            _afterSaveActions.Add(action);
            return (TFactory)this;
        }
        
        public new TFactory AddTo(IList<Factory> list)
        {
            list.Add(this);
            return (TFactory)this;
        }
    }

    /// <summary>
    /// Factory for mutable types.
    /// </summary>
    public abstract class MutableFactory<TModel, TFactory> : Factory<TModel, TFactory> where TModel : class where TFactory : MutableFactory<TModel, TFactory>
    {
        private readonly List<Action<ISession, TModel>> _withActions = new List<Action<ISession, TModel>>();
        //protected TModel Instance { get; set; }

        protected override TModel CreateInstance(ISession session)
        {
            var instance = CreateInstance(NextNumber());
            foreach (var with in _withActions) {
                with(session, instance);
            }

            PerformActions(session, instance);
            AddReferences(session, instance);
            return SaveInstance(session, instance);
        }

        public TFactory With([NotNull] Action<TModel> with)
        {
            _withActions.Add((session, model) => with(model));
            return (TFactory)this;
        }

        public TFactory With([NotNull] Action<ISession, TModel> with)
        {
            _withActions.Add(with);
            return (TFactory)this;
        }

        public new TFactory AddTo(IList<Factory> list)
        {
            list.Add(this);
            return (TFactory)this;
        }

        protected abstract TModel CreateInstance(int number);

        protected virtual void AddReferences(ISession session, TModel instance)
        {
        }

        protected virtual TModel SaveInstance(ISession session, TModel instance)
        {
            session.Save(instance);
            return instance;
        }
    }
