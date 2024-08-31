using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Specifies that an enum should be mapped as its underlying integer value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum)]
    public class UseIntegerAttribute : Attribute
    {
    }
}
