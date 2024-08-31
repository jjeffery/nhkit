using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;
using System.Reflection;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention to use dynamic SQL for inserts. By default, dynamic inserts are used unless disabled.
/// </summary>
[UsedImplicitly]
public class DynamicInsertConvention : IClassConvention, IClassConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
    {
        criteria.Expect(x => {
            if (x.EntityType.GetCustomAttribute(typeof(DynamicInsertAttribute)) is DynamicInsertAttribute attribute) {
                return attribute.Enabled;
            }
                
            // No attribute means use dynamic insert
            return true;
        });
    }


    public void Apply(IClassInstance instance)
    {
        instance.DynamicInsert();
    }
}
