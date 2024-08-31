using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;
using System;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention for many-to-one relationships that have dangling references in the DB.
/// </summary>
[UsedImplicitly]
public class IgnoreReferenceNotFoundConvention : IReferenceConvention, IReferenceConventionAcceptance
{
    public void Apply(IManyToOneInstance instance)
    {
        instance.NotFound.Ignore();
    }

    public void Accept(IAcceptanceCriteria<IManyToOneInspector> criteria)
    {
        criteria.Expect(x => Attribute.IsDefined(x.Property.PropertyType, typeof(IgnoreNotFoundAttribute)) ||
                             Attribute.IsDefined(x.Property.MemberInfo, typeof(IgnoreNotFoundAttribute)));
    }
}
