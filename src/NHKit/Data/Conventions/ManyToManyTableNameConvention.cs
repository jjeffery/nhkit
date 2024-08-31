using FluentNHibernate.Conventions.Inspections;
using JetBrains.Annotations;
using NHKit.Data.AutoMapping;
using System;

namespace NHKit.Data.Conventions;

[UsedImplicitly]
public class ManyToManyTableNameConvention : FluentNHibernate.Conventions.ManyToManyTableNameConvention
{
    protected override string GetBiDirectionalTableName(IManyToManyCollectionInspector collection, IManyToManyCollectionInspector otherSide)
    {
        var entity1 = Reflector.TableName(collection.EntityType);
        var entity2 = Reflector.TableName(otherSide.EntityType);
        if (StringComparer.OrdinalIgnoreCase.Compare(entity1, entity2) < 0) {
            return $"{entity1}_{entity2}";
        }

        return $"{entity2}_{entity1}";
    }

    protected override string GetUniDirectionalTableName(IManyToManyCollectionInspector collection)
    {
        var entity1 = Reflector.TableName(collection.EntityType);
        var entity2 = Reflector.TableName(collection.ChildType);
        if (StringComparer.OrdinalIgnoreCase.Compare(entity1, entity2) < 0)
        {
            return $"{entity1}_{entity2}";
        }

        return $"{entity2}_{entity1}";
    }
}
