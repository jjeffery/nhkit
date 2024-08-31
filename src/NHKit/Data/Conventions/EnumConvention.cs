using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NHKit.Data.Annotations;
using NHKit.Data.Conversions;
using System;
using System.Data.Common;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention for mapping enum values to DB values.
/// </summary>
/// <remarks>
/// By default, enums are stored in the DB as strings, unless <see cref="UseIntegerAttribute"/> is specified.
/// </remarks>
[UsedImplicitly]
public class EnumConvention : IPropertyConvention, IPropertyConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x => x.Property.PropertyType.IsEnum || IsNullableEnum(x.Property.PropertyType));
    }

    public void Apply(IPropertyInstance instance)
    {
        var useInteger = Attribute.IsDefined(instance.Property.MemberInfo, typeof(UseIntegerAttribute))
                         || Attribute.IsDefined(GetEnumType(instance.Property.PropertyType), typeof(UseIntegerAttribute));
        if (useInteger) {
            instance.CustomType(instance.Property.PropertyType);
        }
        else {
            var enumUserType = typeof(UserType<>).MakeGenericType(instance.Property.PropertyType);
            instance.CustomType(enumUserType);
        }
    }

    private static bool IsNullableEnum(Type t)
    {
        return Nullable.GetUnderlyingType(t)?.IsEnum ?? false;
    }

    private static Type GetEnumType(Type t)
    {
        return Nullable.GetUnderlyingType(t) ?? t;
    }
    
    [Serializable]
    public class UserType<T> : IUserType
    {
        public SqlType[] SqlTypes { get; }
        public Type ReturnedType { get; }
        public bool IsMutable { get; }
        public bool IsNullable { get; }

        public UserType()
        {
            Type enumType = Nullable.GetUnderlyingType(typeof(T));
            if (enumType == null) {
                enumType = typeof(T);
            }
            else {
                IsNullable = true;
            }
            if (!enumType.IsEnum) {
                throw new MappingException($"{enumType.Name} does not inherit from System.Enum");
            }

            SqlTypes = [SqlTypeFactory.GetString(EnumStringConverter<T>.MaxLength)];
            ReturnedType = typeof(T);
            IsMutable = false;
        }

        bool IUserType.Equals(object x, object y)
        {
            if (x == null && y == null) {
                return true;
            }

            if (x == null || y == null) {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            if (x == null) {
                return 0;
            }

            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            object obj = NHibernateUtil.String.NullSafeGet(rs, names[0], session);
            if (obj == null) {
                return null;
            }

            var s = (string) obj;
            if (IsNullable && string.IsNullOrWhiteSpace(s)) {
                // This is a special condition for MySQL enum types.
                // When the database is not in strict mode and a value is inserted/updated that
                // is not one of the valid enum values, a blank will be entered.
                // As a work-around, if we receive a blank and the enum type is nullable,
                // treat as a null
                return null;
            }

            return EnumStringConverter<T>.ConvertToEnum(s);
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            string s = null;
            if (value is T t) {
                s = EnumStringConverter<T>.ConvertToString(t);
            }

            if (s == null) {
                cmd.Parameters[index].Value = DBNull.Value;
            }
            else {
                cmd.Parameters[index].Value = s;
            }
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }
    }
}
