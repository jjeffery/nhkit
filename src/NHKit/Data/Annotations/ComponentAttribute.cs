using System;

namespace NHKit.Data.Annotations {
    /// <summary>
    /// Used to indicate that a class is persisted as a component in a persisted class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute;
}