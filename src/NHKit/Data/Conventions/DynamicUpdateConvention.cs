using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;
using System.Reflection;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention to use dynamic SQL for updates. By default, dynamic updates are used unless disabled.
/// </summary>
[UsedImplicitly]
public class DynamicUpdateConvention : IClassConvention, IClassConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
    {
        criteria.Expect(x => {
            if (x.EntityType.GetCustomAttribute(typeof(DynamicUpdateAttribute)) is DynamicUpdateAttribute attribute) {
                return attribute.Enabled;
            }

            // No attribute means use dynamic update.
            return true;
        });
    }

    public void Apply(IClassInstance instance)
    {
        instance.DynamicUpdate();
    }
}
