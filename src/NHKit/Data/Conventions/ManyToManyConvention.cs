using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class ManyToManyConvention : IHasManyToManyConvention
{
    public void Apply(IManyToManyCollectionInstance instance)
    {
        instance.BatchSize(128);
    }
}
