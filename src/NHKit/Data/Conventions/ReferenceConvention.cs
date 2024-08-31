using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.Mapping;
using JetBrains.Annotations;
using NHKit.Data.AutoMapping;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class ReferenceConvention : IReferenceConvention 
{
    public void Apply(IManyToOneInstance instance)
    {
        var tableName = Reflector.TableName(instance.Property.DeclaringType);
        var columnName = Reflector.ColumnName(instance.Property.MemberInfo);

        instance.ForeignKey($"{tableName}_{columnName}_fk");
        instance.Index($"{tableName}_{columnName}_idx");

        // If the thing we are referring to is abstract, then do not use proxies.
        // This way we can cast it without throwing an exception.
        if (instance.Class.GetUnderlyingSystemType().IsAbstract) {
            instance.LazyLoad(Laziness.NoProxy);
        }
    }
}
