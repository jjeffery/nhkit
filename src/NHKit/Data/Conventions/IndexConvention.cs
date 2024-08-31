using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;
using NHKit.Data.AutoMapping;
using System;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention for adding one or more indexes to a column.
/// </summary>
[UsedImplicitly]
public class IndexConvention : IPropertyConvention, IPropertyConventionAcceptance
{
    public void Accept(IAcceptanceCriteria<IPropertyInspector> criteria)
    {
        // want one or more index attributes on the property
        criteria.Expect(
            x => Attribute.GetCustomAttributes(x.Property.MemberInfo, typeof(IndexAttribute)).Length > 0
        );
    }

    public void Apply(IPropertyInstance instance)
    {
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (IndexAttribute attribute in Attribute.GetCustomAttributes(instance.Property.MemberInfo, typeof(IndexAttribute))) {
            var indexName = attribute.Name;
            if (string.IsNullOrWhiteSpace(indexName)) {
                var tableName = Reflector.TableName(instance.Property.DeclaringType);
                var columnName = Reflector.ColumnName(instance.Property.MemberInfo);
                indexName = $"{tableName}_{columnName}_idx";
            }

            instance.Index(indexName);
        }
    }
}
