using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHibernate.Type;
using NHKit.Data.Annotations;
using System;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention to use the DATE type for a column if the property has the <see cref="DateAttribute"/> attribute. 
/// </summary>
[UsedImplicitly]
public class DateConvention : IPropertyConvention, IPropertyConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(
            x => Attribute.GetCustomAttribute(x.Property.MemberInfo, typeof(DateAttribute)) != null
        );
    }

    public void Apply(IPropertyInstance instance)
    {
        instance.CustomType<DateType>();
    }
}
