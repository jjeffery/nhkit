using System;

namespace NHKit.Data.Annotations
{
    /// <summary>
    /// Indicates that a unique index should be created for the column associated
    /// with this property. A multi-column unique index can be created by specifying
    /// the same index name for multiple properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UniqueIndexAttribute : Attribute
    {
        public string KeyName { get; private set; }
        public UniqueIndexAttribute(string keyName = null)
        {
            KeyName = keyName;
        }
    }
}
