using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.AutoMapping;

namespace NHKit.Data.Conventions;

/// <summary>
/// Convention used to obtain the column name for a property.
/// </summary>
[UsedImplicitly]
public class ColumnNameConvention : IPropertyConvention
{
    public void Apply(IPropertyInstance instance)
    {
        var columnName = Reflector.ColumnName(instance.Property.MemberInfo);
        instance.Column(columnName);
    }
}
