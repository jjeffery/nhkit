using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHKit.Data.Annotations;
using NHKit.Data.AutoMapping;
using System;
using System.Reflection;

namespace NHKit.Data.Conventions
{
    /// <summary>
    /// Foreign key convention.
    /// </summary>
    /// <remarks>
    /// Note that this class does not inherit from Fluent NHibernate's ForeignKeyConvention class
    /// as that class is too simple to handle the configuration of many to one collections.
    /// </remarks>
    public class ForeignKeyConvention 
        : IReferenceConvention, IHasManyToManyConvention, IJoinedSubclassConvention, IJoinConvention, ICollectionConvention
    {
        public void Apply(IManyToOneInstance instance)
        {
            var columnName = Reflector.ColumnName(instance.Property.MemberInfo);
            instance.Column(columnName);
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            var keyColumn = Reflector.ColumnName(instance.EntityType);
            var childColumn = Reflector.ColumnName(instance.ChildType);

            instance.Key.Column(keyColumn);
            instance.Relationship.Column(childColumn);
        }

        public void Apply(IJoinedSubclassInstance instance)
        {
            if (instance.Type.BaseType != null) {
                var columnName = Reflector.ColumnName(instance.Type.BaseType);
                instance.Key.Column(columnName);
            }
        }

        public void Apply(IJoinInstance instance)
        {
            var columnName = Reflector.ColumnName(instance.EntityType);
            instance.Key.Column(columnName);
        }

        public void Apply(ICollectionInstance instance)
        {
            var foreignKeyColumnAttribute = Attribute.GetCustomAttribute(
                instance.Member,
                typeof(ForeignKeyColumnAttribute)
            ) as ForeignKeyColumnAttribute;

            var columnName = foreignKeyColumnAttribute?.Name;

            if (columnName == null) {
                // attempt to find the property
                var childType = instance.ChildType;
                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (var member in childType.GetMembers()) {
                    if (member is PropertyInfo property) {
                        if (property.PropertyType == instance.EntityType) {
                            if (columnName == null) {
                                columnName = Reflector.ColumnName(member);
                            }
                            else {
                                // if columnName has already been specified, this means that there are
                                // multiple properties with the entity type, so it is not possible to
                                // determine which column to use
                                columnName = null;
                                break;
                            }
                        }
                    }
                }
            }

            if (columnName == null) {
                columnName = Reflector.ColumnName(instance.EntityType);
            }

            instance.Key.Column(columnName);
        }
    }
}
