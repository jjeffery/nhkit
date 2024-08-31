using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHibernate.Type;
using NHKit.Data.Annotations;
using System;

namespace NHKit.Data.Conventions
{
    [UsedImplicitly]
    public class UtcDateTimeConvention : IPropertyConvention, IPropertyConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
        {
            criteria.Expect(
                x => Attribute.GetCustomAttribute(x.Property.MemberInfo, typeof(UtcDateTimeAttribute)) != null
            );
        }

        public void Apply(IPropertyInstance instance)
        {
            instance.CustomType<UtcDateTimeType>();
        }
    }
}
