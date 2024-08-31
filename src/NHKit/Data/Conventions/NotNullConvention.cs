using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class NotNullConvention : AttributePropertyConvention<RequiredAttribute>
{
    protected override void Apply(RequiredAttribute attribute, IPropertyInstance instance)
    {
        instance.Not.Nullable();
    }
}

[UsedImplicitly]
public class ReferenceNotNullConvention : IReferenceConventionAcceptance, IReferenceConvention
{
    public void Accept(IAcceptanceCriteria<IManyToOneInspector> criteria)
    {
        criteria.Expect(x =>
            Attribute.GetCustomAttribute(x.Property.MemberInfo, typeof(RequiredAttribute)) != null);
    }

    public void Apply(IManyToOneInstance instance)
    {
        instance.Not.Nullable();
    }
}
