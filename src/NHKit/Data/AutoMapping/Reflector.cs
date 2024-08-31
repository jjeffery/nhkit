using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NHKit.Data.AutoMapping;

/// <summary>
/// Common reflection-based utility methods for auto mapping.
/// </summary>
public static class Reflector
{
    public static string ToSnakeCase([NotNull] string s)
    {
        // UpperCase => upper_case
        string snakeCase = Regex.Replace(s, "^([A-Z]{2})([A-Z])", "$1_$2"); // eg DBName => DB_Name
        snakeCase = Regex.Replace(snakeCase, "([^A-Z_]+)([A-Z]+)", "$1_$2");
        snakeCase = snakeCase.ToLower();
        return snakeCase;
    }

    public static string ToKebabCase([NotNull] string s)
    {
        // UpperCase => upper-case
        string kebabCase = Regex.Replace(s, "([^A-Z_-])([A-Z]+)", "$1-$2");
        kebabCase = kebabCase.ToLower();
        return kebabCase;
    }

    public static string ToCamelCase([NotNull] string s)
    {
        if (s.Length == 0) {
            return s;
        }

        if (s.Length == 1) {
            return s.ToLower();
        }

        // UpperCase => upperCase
        return s.Substring(0, 1).ToLower() + s.Substring(1);
    }

    public static bool IsPersistent([NotNull] Type type)
    {
        return type.CustomAttributes.Any(c => c.AttributeType == typeof(TableAttribute));
    }

    public static string TableName([NotNull] Type t)
    {
        var attribute = t.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
        var tableName = attribute?.Name;
        if (string.IsNullOrWhiteSpace(tableName)) {
            tableName = ToSnakeCase(t.Name);
        }

        return tableName;
    }

    public static string ColumnName<TEntity, TPropertyType>(Expression<Func<TEntity, TPropertyType>> expression)
    {
        var member = ((MemberExpression)expression.Body).Member;
        return ColumnName(member);
    }

    public static string ColumnName([NotNull] MemberInfo m)
    {
        var columnName = ColumnNameFromAnnotation(m);
        if (columnName == null) {
            columnName = ToSnakeCase(m.Name);

            if (m is PropertyInfo property) {
                if (IsPersistent(property.PropertyType)) {
                    // the property is a reference in a one-to-many
                    columnName += "_id";
                }
            } 
        }

        return columnName;
    }

    /// <summary>
    /// Used as a default column name in many-to-many relationships.
    /// </summary>
    public static string ColumnName([NotNull] Type type)
    {
        var columnName = ToSnakeCase(type.Name) + "_id";
        return columnName;
    }

    private static string ColumnNameFromAnnotation([NotNull] MemberInfo m)
    {
        var attribute = Attribute.GetCustomAttribute(m, typeof(ColumnAttribute)) as ColumnAttribute;
        var columnName = attribute?.Name;
        if (string.IsNullOrWhiteSpace(columnName)) {
            columnName = null;
        }

        return columnName;
    }

    public static string UniquePrefix([NotNull] Type type)
    {
        var prefix = UniquePrefixFromAttribute(type);
        if (string.IsNullOrWhiteSpace(prefix)) {
            prefix = ToKebabCase(type.Name);
        }

        return prefix;
    }

    private static string UniquePrefixFromAttribute([NotNull] Type t)
    {
        // TODO(jpj): define a new attribute for a persistent class that specifies a unique
        // prefix for that class. For example [UniquePrefix("ph")]
        return null;
    }
}
