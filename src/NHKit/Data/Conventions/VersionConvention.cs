using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class VersionConvention : IVersionConvention
{
    public void Apply(IVersionInstance instance)
    {
        // TODO: would prefer to use inflector here but it is not
        // obvious how to get the property name.
        instance.Column("lock_version");
    }
}
