using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;
using System.Reflection;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention for classes whose instances are stored in the second-level cache.
/// </summary>
[UsedImplicitly]
public class CachedConvention : IClassConvention, IClassConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
    {
        criteria.Expect(x => x.EntityType.GetCustomAttribute(typeof(CachedAttribute)) != null);
    }

    public void Apply(IClassInstance instance)
    {
        instance.Cache.ReadOnly();
        instance.ReadOnly();
    }
}
