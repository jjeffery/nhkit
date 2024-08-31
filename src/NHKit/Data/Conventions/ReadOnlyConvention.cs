using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;
using System.Reflection;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class ReadOnlyConvention : IPropertyConvention, IPropertyConventionAcceptance, IClassConvention, IClassConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        criteria.Expect(x => x.Property.MemberInfo.GetCustomAttribute(typeof(ReadOnlyAttribute)) != null);
    }

    public void Apply(IPropertyInstance instance)
    {
        instance.ReadOnly();
    }
        
    public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
    {
        criteria.Expect(x => x.EntityType.GetCustomAttribute(typeof(ReadOnlyAttribute)) != null);
    }

    public void Apply(IClassInstance instance)
    {
        var attribute = (ReadOnlyAttribute) instance.EntityType.GetCustomAttribute(typeof(ReadOnlyAttribute));
        instance.ReadOnly();
    }
}
