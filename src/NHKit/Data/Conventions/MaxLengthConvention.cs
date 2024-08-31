using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class MaxLengthConvention : IPropertyConvention, IPropertyConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(
            x => Attribute.GetCustomAttribute(x.Property.MemberInfo, typeof(MaxLengthAttribute)) != null
        );
    }

    public void Apply(IPropertyInstance instance)
    {
        if (Attribute.GetCustomAttribute(instance.Property.MemberInfo, typeof(MaxLengthAttribute)) is MaxLengthAttribute attribute) {
            instance.Length(attribute.Length);
        }
    }
}
