using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using JetBrains.Annotations;
using NHKit.Data.AutoMapping;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace NHKit.Data.Conventions
{
    [UsedImplicitly]
    public class TableNameConvention : IClassConvention, IClassConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
        {
            criteria.Expect(x => x.EntityType.GetCustomAttribute(typeof(TableAttribute)) != null);
        }

        public void Apply(IClassInstance instance)
        {
            var tableName = Reflector.TableName(instance.EntityType);
            instance.Table(tableName);
        }
    }
}
