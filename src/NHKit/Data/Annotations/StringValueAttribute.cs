using System;
using JetBrains.Annotations;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Specify a string value to be stored in the database for an enumeration field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute : Attribute
    {
        public string Value { get; }

        public StringValueAttribute([CanBeNull] string value)
        {
            Value = value;
        }
    }
}