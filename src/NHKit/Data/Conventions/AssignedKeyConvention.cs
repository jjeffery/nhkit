using System.Reflection;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class AssignedKeyConvention : IIdConvention, IIdConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IIdentityInspector> criteria)
    {
        criteria.Expect(x => x.EntityType.GetCustomAttribute(typeof(AssignedKeyAttribute)) != null);
    }

    public void Apply(IIdentityInstance instance)
    {
        instance.GeneratedBy.Assigned();
    }
}
