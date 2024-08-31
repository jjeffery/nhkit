using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Indicates that the entity class has an assigned key, as opposed to a generated key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssignedKeyAttribute : Attribute;
}