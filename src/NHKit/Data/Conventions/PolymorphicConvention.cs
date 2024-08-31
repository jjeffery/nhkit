using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;
using NHKit.Data.AutoMapping;
using NHKit.Data.Types;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class PolymorphicConvention : IUserTypeConvention
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x => typeof(Polymorphic).IsAssignableFrom(x.Type.GetUnderlyingSystemType()));
    }

    public void Apply(IPropertyInstance instance)
    {
        var prefix = Reflector.ColumnName(instance.Property.MemberInfo) + "_";
        var instanceType = instance.Type.GetUnderlyingSystemType();
        var userType = typeof(UserType<>).MakeGenericType(instanceType);
        instance.CustomType(userType, prefix);
    }
        
    public class UserType<T> : ICompositeUserType where T : Polymorphic
    {
        public string[] PropertyNames => ["id", "type"];
        public IType[] PropertyTypes => [NHibernateUtil.Int32, NHibernateUtil.String];
        public Type ReturnedClass => typeof(T);
        public bool IsMutable => false;
        private readonly IReadOnlyDictionary<string, Type> _types;

        public UserType()
        {
            var objectType = typeof(T);
            var members = objectType.GetMember("Types", MemberTypes.Field, BindingFlags.Static | BindingFlags.Public);
            if (members.Length == 0) {
                throw new ArgumentException($"Missing \"Types\" field on type {objectType}: should be static readonly");
            }

            try {
                var field = (FieldInfo) members[0];
                _types = (IReadOnlyDictionary<string, Type>)field.GetValue(null);
            }
            catch (Exception) {
                throw new ArgumentException($"The \"Types\" member on {objectType} should be a static readonly IReadOnlyDictionary<string, Type> field");
            }
        }

        public object GetPropertyValue(object component, int property)
        {
            if (component == null)
            {
                return null;
            }

            var v = (Polymorphic)component;
            switch (property)
            {
                case 0:
                    return v.Id;
                case 1:
                    return v.Type;
                default:
                    throw new ArgumentException($"Invalid property value: {property}");
            }
        }

        public void SetPropertyValue(object component, int property, object value)
        {
            throw new InvalidOperationException($"{typeof(T)} is an immutable class");
        }

        bool ICompositeUserType.Equals(object x, object y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x?.GetHashCode() ?? 0;
        }

        public object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner)
        {
            var idObj = NHibernateUtil.Int32.NullSafeGet(dr, names[0], session, owner);
            var typeObj = NHibernateUtil.String.NullSafeGet(dr, names[1], session, owner);

            if (idObj is int id && typeObj != null && typeObj is string type) {
                if (!_types.TryGetValue(type, out var objectType)) {
                    throw new ApplicationException($"Unsupported type for {typeof(T)}: {type}");
                }

                if (session is ISession statefulSession) {
                    // add the object to the first-level cache, but do not fetch from the DB
                    statefulSession.Load(objectType, id);
                }

                return Activator.CreateInstance(typeof(T), BindingFlags.Default, null, new object[] {id, type}, null);
            }

            return null;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            var values = new object[2];

            if (value == null) {
                values[0] = DBNull.Value;
                values[1] = DBNull.Value;
            }
            else {
                var v = (Polymorphic) value;
                values[0] = v.Id;
                values[1] = v.Type;
            }

            for (var i = 0; i < values.Length; i++) {
                if (settable[i]) {
                    cmd.Parameters[index++].Value = values[i];
                }
            }
        }

        public object DeepCopy(object value)
        {
            // immutable object, can just return value
            return value;
        }

        public object Disassemble(object value, ISessionImplementor session)
        {
            // immutable object, can just return value
            return value;
        }

        public object Assemble(object cached, ISessionImplementor session, object owner)
        {
            // immutable object, can just return cached
            return cached;
        }

        public object Replace(object original, object target, ISessionImplementor session, object owner)
        {
            // immutable object, can just return original
            return original;
        }
    }
}
