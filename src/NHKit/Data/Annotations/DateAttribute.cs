using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Indicates that the associated column type should be DATE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateAttribute : Attribute;
}