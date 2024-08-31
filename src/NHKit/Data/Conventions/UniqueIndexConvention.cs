using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.Annotations;
using NHKit.Data.AutoMapping;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class UniqueIndexConvention : AttributePropertyConvention<UniqueIndexAttribute>
{
    // TODO: handle multiple unique keys -- see APM's code.
    protected override void Apply(UniqueIndexAttribute attribute, IPropertyInstance instance)
    {
        var keyName = attribute.KeyName;
        if (string.IsNullOrEmpty(keyName)) {
            var tableName = Reflector.TableName(instance.Property.DeclaringType);
            var columnName = instance.Name;
            keyName = $"UK_{tableName}_{columnName}";
        }

        instance.UniqueKey(keyName);
    }
}
