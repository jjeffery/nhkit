using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Specify whether to use dynamic SQL for insert statements.
    /// </summary>
    /// <remarks>
    /// Dynamic insert is on by default. Use this attribute to turn it off.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class DynamicInsertAttribute : Attribute
    {
        public bool Enabled { get; }

        public DynamicInsertAttribute(bool enabled)
        {
            Enabled = enabled;
        }
    }
}