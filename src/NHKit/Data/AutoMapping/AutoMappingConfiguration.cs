using FluentNHibernate;
using FluentNHibernate.Automapping;
using NHKit.Data.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace NHKit.Data.AutoMapping;

public class AutomappingConfiguration : DefaultAutomappingConfiguration
{
    /// <summary>
    /// Specifies the criteria that types must meet in order to be mapped.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>True if type should be mapped, false otherwise.</returns>
    public override bool ShouldMap(Type type)
    {
        return Reflector.IsPersistent(type);
    }

    public override bool IsComponent(Type type)
    {
        return type.CustomAttributes.Any(c => c.AttributeType == typeof(ComponentAttribute));
    }

    public override string GetComponentColumnPrefix(Member member)
    {
        // TODO(jpj): if we start using components we will need to have a ComponentPrefixAttribute,
        // because component prefixes are not consistent anywhere.
        return Reflector.ToSnakeCase(member.Name) + "_";
    }

    public override bool IsId(Member member)
    {
        if (Attribute.GetCustomAttribute(member.MemberInfo, typeof(KeyAttribute)) != null) {
            return true;
        }

        return base.IsId(member);
    }

    public override bool IsVersion(Member member)
    {
        var validNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "LockVersion", "lock_version" };
        var validTypes = new HashSet<Type> { typeof(int), typeof(long) };
        return validNames.Contains(member.Name) && validTypes.Contains(member.PropertyType);
    }

    public override bool ShouldMap(Member member)
    {
        if (!base.ShouldMap(member)) {
            return false;
        }

        if (!member.CanWrite) {
            return false;
        }

        // pay attention to the [NotMapped] attribute in System.ComponentModel.DataAnnotations.Schema
        return member.MemberInfo.CustomAttributes.All(c => c.AttributeType != typeof(NotMappedAttribute));
    }
}
