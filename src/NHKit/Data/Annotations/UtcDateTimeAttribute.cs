using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Indicates that the associated column type is datetime, and the corresponding DateTime value is UTC.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UtcDateTimeAttribute : Attribute
    {
    }
}
