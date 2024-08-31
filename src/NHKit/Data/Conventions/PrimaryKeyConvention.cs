using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.AutoMapping;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class PrimaryKeyConvention : IIdConvention
{
    public void Apply(IIdentityInstance instance)
    {
        var columnName = GetAttribute<ColumnAttribute>(instance)?.Name;
        if (string.IsNullOrWhiteSpace(columnName)) {
            instance.Column(Reflector.ToSnakeCase(instance.Property.Name));
        }
        else {
            instance.Column(columnName);
        }

        // useful for varchar primary keys
        var maxLength = GetAttribute<MaxLengthAttribute>(instance)?.Length;
        if (maxLength != null) {
            instance.Length(maxLength.Value);
        }
    }

    private static T GetAttribute<T>(IIdentityInstance instance) where T : Attribute
    {
        return Attribute.GetCustomAttribute(instance.Property.MemberInfo, typeof(T)) as T;
    }
}
